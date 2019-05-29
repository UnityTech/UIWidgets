using System;
using System.IO;
using System.Text;

namespace Unity.UIWidgets.ui {
    // from https://github.com/avianbc/NGif/blob/master/Components/GifDecoder.cs
    // https://gist.github.com/devunwired/4479231
    // No DISPOSAL_PREVIOUS as its not actually widely used.
    public class GifDecoder : IDisposable {
        /**
         * File read status: No errors.
         */
        public const int STATUS_OK = 0;

        /**
         * File read status: Error decoding file (may be partially decoded)
         */
        public const int STATUS_FORMAT_ERROR = 1;

        /**
         * File read status: Unable to open source.
         */
        public const int STATUS_OPEN_ERROR = 2;

        // max decoder pixel stack size
        const int MAX_STACK_SIZE = 4096;

        // input stream
        Stream _inStream;

        /**
        * Global status code of GIF data parsing
        */
        int _status;

        // Global File Header values and parsing flags
        volatile int _width; // full image width
        volatile int _height; // full image height
        bool _gctFlag; // global color table used
        int _gctSize; // size of global color table
        volatile int _loopCount = 1; // iterations; 0 = repeat forever

        int[] _gct; // global color table
        int[] _lct; // local color table
        int[] _act; // active color table

        int _bgIndex; // background color index
        int _bgColor; // background color
        int _lastBgColor; // previous bg color
        int _pixelAspect; // pixel aspect ratio

        bool _lctFlag; // local color table flag
        bool _interlace; // interlace flag
        int _lctSize; // local color table size

        int _ix, _iy, _iw, _ih; // current image rectangle
        int _lix, _liy, _liw, _lih; // last image rect
        int[] _image; // current frame

        byte[] _block = new byte[256]; // current data block
        int _blockSize = 0; // block size

        // last graphic control extension info
        int _dispose = 0;

        // 0=no action; 1=leave in place; 2=restore to bg; 3=restore to prev
        int _lastDispose = 0;
        bool _transparency = false; // use transparent color
        int _delay = 0; // delay in milliseconds
        int _transIndex; // transparent color index

        // LZW decoder working arrays
        short[] _prefix;
        byte[] _suffix;
        byte[] _pixelStack;
        byte[] _pixels;

        volatile GifFrame _currentFrame; // frames read from current file
        volatile int _frameCount;
        volatile bool _done;

        public class GifFrame {
            public byte[] bytes;
            public int delay;
        }

        public int frameWidth {
            get { return this._width; }
        }

        public int frameHeight {
            get { return this._height; }
        }

        public GifFrame currentFrame {
            get { return this._currentFrame; }
        }

        public int frameCount {
            get { return this._frameCount; }
        }

        public int loopCount {
            get { return this._loopCount; }
        }

        public bool done {
            get { return this._done; }
        }

        void _setPixels() {
            // fill in starting image contents based on last image's dispose code
            if (this._lastDispose > 0) {
                var n = this._frameCount - 1;
                if (n > 0) {
                    if (this._lastDispose == 2) {
                        // fill last image rect area with background color 
                        var fillcolor = this._transparency ? 0 : this._lastBgColor;
                        for (var i = 0; i < this._lih; i++) {
                            var line = i + this._liy;
                            if (line >= this._height) {
                                continue;
                            }

                            line = this._height - line - 1;
                            var dx = line * this._width + this._lix;
                            var endx = dx + this._liw;
                            while (dx < endx) {
                                this._image[dx++] = fillcolor;
                            }
                        }
                    }
                }
            }

            // copy each source line to the appropriate place in the destination
            int pass = 1;
            int inc = 8;
            int iline = 0;
            for (int i = 0; i < this._ih; i++) {
                int line = i;
                if (this._interlace) {
                    if (iline >= this._ih) {
                        pass++;
                        switch (pass) {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                        }
                    }

                    line = iline;
                    iline += inc;
                }

                line += this._iy;
                if (line >= this._height) {
                    continue;
                }

                var sx = i * this._iw;
                line = this._height - line - 1;
                var dx = line * this._width + this._ix;
                var endx = dx + this._iw;

                for (; dx < endx; dx++) {
                    var c = this._act[this._pixels[sx++] & 0xff];
                    if (c != 0) {
                        this._image[dx] = c;
                    }
                }
            }
        }

        /**
         * Reads GIF image from stream
         *
         * @param BufferedInputStream containing GIF file.
         * @return read status code (0 = no errors)
         */
        public int read(Stream inStream) {
            this._init();
            if (inStream != null) {
                this._inStream = inStream;
                this._readHeader();
            }
            else {
                this._status = STATUS_OPEN_ERROR;
            }

            return this._status;
        }

        public void Dispose() {
            if (this._inStream != null) {
                this._inStream.Dispose();
                this._inStream = null;
            }
        }

        /**
         * Decodes LZW image data into pixel array.
         * Adapted from John Cristy's ImageMagick.
         */
        void _decodeImageData() {
            const int NullCode = -1;
            int npix = this._iw * this._ih;
            int available,
                clear,
                code_mask,
                code_size,
                end_of_information,
                in_code,
                old_code,
                bits,
                code,
                count,
                i,
                datum,
                data_size,
                first,
                top,
                bi,
                pi;

            if ((this._pixels == null) || (this._pixels.Length < npix)) {
                this._pixels = new byte[npix]; // allocate new pixel array
            }

            if (this._prefix == null) {
                this._prefix = new short[MAX_STACK_SIZE];
            }

            if (this._suffix == null) {
                this._suffix = new byte[MAX_STACK_SIZE];
            }

            if (this._pixelStack == null) {
                this._pixelStack = new byte[MAX_STACK_SIZE + 1];
            }

            //  Initialize GIF data stream decoder.

            data_size = this._read();
            clear = 1 << data_size;
            end_of_information = clear + 1;
            available = clear + 2;
            old_code = NullCode;
            code_size = data_size + 1;
            code_mask = (1 << code_size) - 1;
            for (code = 0; code < clear; code++) {
                this._prefix[code] = 0;
                this._suffix[code] = (byte) code;
            }

            //  Decode GIF pixel stream.

            datum = bits = count = first = top = pi = bi = 0;

            for (i = 0; i < npix;) {
                if (top == 0) {
                    if (bits < code_size) {
                        //  Load bytes until there are enough bits for a code.
                        if (count == 0) {
                            // Read a new data block.
                            count = this._readBlock();
                            if (count <= 0) {
                                break;
                            }

                            bi = 0;
                        }

                        datum += (this._block[bi] & 0xff) << bits;
                        bits += 8;
                        bi++;
                        count--;
                        continue;
                    }

                    //  Get the next code.

                    code = datum & code_mask;
                    datum >>= code_size;
                    bits -= code_size;

                    //  Interpret the code

                    if ((code > available) || (code == end_of_information)) {
                        break;
                    }

                    if (code == clear) {
                        //  Reset decoder.
                        code_size = data_size + 1;
                        code_mask = (1 << code_size) - 1;
                        available = clear + 2;
                        old_code = NullCode;
                        continue;
                    }

                    if (old_code == NullCode) {
                        this._pixelStack[top++] = this._suffix[code];
                        old_code = code;
                        first = code;
                        continue;
                    }

                    in_code = code;
                    if (code == available) {
                        this._pixelStack[top++] = (byte) first;
                        code = old_code;
                    }

                    while (code > clear) {
                        this._pixelStack[top++] = this._suffix[code];
                        code = this._prefix[code];
                    }

                    first = this._suffix[code] & 0xff;

                    //  Add a new string to the string table,

                    if (available >= MAX_STACK_SIZE) {
                        break;
                    }

                    this._pixelStack[top++] = (byte) first;
                    this._prefix[available] = (short) old_code;
                    this._suffix[available] = (byte) first;
                    available++;
                    if (((available & code_mask) == 0)
                        && (available < MAX_STACK_SIZE)) {
                        code_size++;
                        code_mask += available;
                    }

                    old_code = in_code;
                }

                //  Pop a pixel off the pixel stack.

                top--;
                this._pixels[pi++] = this._pixelStack[top];
                i++;
            }

            for (i = pi; i < npix; i++) {
                this._pixels[i] = 0; // clear missing pixels
            }
        }

        /**
         * Returns true if an error was encountered during reading/decoding
         */
        bool _error() {
            return this._status != STATUS_OK;
        }

        /**
         * Initializes or re-initializes reader
         */
        void _init() {
            this._status = STATUS_OK;
            this._currentFrame = null;
            this._frameCount = 0;
            this._done = false;
            this._gct = null;
            this._lct = null;
        }

        /**
         * Reads a single byte from the input stream.
         */
        int _read() {
            int curByte = 0;
            try {
                curByte = this._inStream.ReadByte();
            }
            catch (IOException) {
                this._status = STATUS_FORMAT_ERROR;
            }

            return curByte;
        }

        /**
         * Reads next variable length block from input.
         *
         * @return number of bytes stored in "buffer"
         */
        int _readBlock() {
            this._blockSize = this._read();
            int n = 0;
            if (this._blockSize > 0) {
                try {
                    int count = 0;
                    while (n < this._blockSize) {
                        count = this._inStream.Read(this._block, n, this._blockSize - n);
                        if (count == -1) {
                            break;
                        }

                        n += count;
                    }
                }
                catch (IOException) {
                }

                if (n < this._blockSize) {
                    this._status = STATUS_FORMAT_ERROR;
                }
            }

            return n;
        }

        /**
         * Reads color table as 256 RGB integer values
         *
         * @param ncolors int number of colors to read
         * @return int array containing 256 colors (packed ARGB with full alpha)
         */
        int[] _readColorTable(int ncolors) {
            int nbytes = 3 * ncolors;
            int[] tab = null;
            byte[] c = new byte[nbytes];
            int n = 0;
            try {
                n = this._inStream.Read(c, 0, c.Length);
            }
            catch (IOException) {
            }

            if (n < nbytes) {
                this._status = STATUS_FORMAT_ERROR;
            }
            else {
                tab = new int[256]; // max size to avoid bounds checks
                int i = 0;
                int j = 0;
                while (i < ncolors) {
                    int r = c[j++] & 0xff;
                    int g = c[j++] & 0xff;
                    int b = c[j++] & 0xff;
                    tab[i++] = (int) (0xff000000 | ((uint) r << 16) | ((uint) g << 8) | (uint) b);
                }
            }

            return tab;
        }

        /**
         * Main file parser.  Reads GIF content blocks.
         */
        public int nextFrame() {
            // read GIF file content blocks
            bool done = false;
            while (!(done || this._error())) {
                int code = this._read();
                switch (code) {
                    case 0x2C: // image separator
                        this._readImage();
                        done = true;
                        break;

                    case 0x21: // extension
                        code = this._read();
                        switch (code) {
                            case 0xf9: // graphics control extension
                                this._readGraphicControlExt();
                                break;

                            case 0xff: // application extension
                                this._readBlock();

                                var appBuilder = new StringBuilder();
                                for (int i = 0; i < 11; i++) {
                                    appBuilder.Append((char) this._block[i]);
                                }

                                string app = appBuilder.ToString();
                                if (app.Equals("NETSCAPE2.0")) {
                                    this._readNetscapeExt();
                                }
                                else {
                                    this._skip(); // don't care
                                }

                                break;

                            default: // uninteresting extension
                                this._skip();
                                break;
                        }

                        break;

                    case 0x3b: // terminator
                        this._done = true;
                        done = true;
                        break;

                    case 0x00: // bad byte, but keep going and see what happens
                        break;

                    default:
                        this._status = STATUS_FORMAT_ERROR;
                        break;
                }
            }

            return this._status;
        }

        /**
         * Reads Graphics Control Extension values
         */
        void _readGraphicControlExt() {
            this._read(); // block size
            int packed = this._read(); // packed fields
            this._dispose = (packed & 0x1c) >> 2; // disposal method
            if (this._dispose == 0) {
                this._dispose = 1; // elect to keep old image if discretionary
            }

            this._transparency = (packed & 1) != 0;
            this._delay = this._readShort() * 10; // delay in milliseconds
            this._transIndex = this._read(); // transparent color index
            this._read(); // block terminator
        }

        /**
         * Reads GIF file header information.
         */
        void _readHeader() {
            var idBuilder = new StringBuilder();
            for (int i = 0; i < 6; i++) {
                idBuilder.Append((char) this._read());
            }

            var id = idBuilder.ToString();
            if (!id.StartsWith("GIF")) {
                this._status = STATUS_FORMAT_ERROR;
                return;
            }

            this._readLSD();
            if (this._gctFlag && !this._error()) {
                this._gct = this._readColorTable(this._gctSize);
                this._bgColor = this._gct[this._bgIndex];
            }

            this._currentFrame = new GifFrame {
                bytes = new byte[this._width * this._height * sizeof(int)],
                delay = 0
            };
        }

        /**
         * Reads next frame image
         */
        void _readImage() {
            this._ix = this._readShort(); // (sub)image position & size
            this._iy = this._readShort();
            this._iw = this._readShort();
            this._ih = this._readShort();

            int packed = this._read();
            this._lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
            this._interlace = (packed & 0x40) != 0; // 2 - interlace flag
            // 3 - sort flag
            // 4-5 - reserved
            this._lctSize = 2 << (packed & 7); // 6-8 - local color table size

            if (this._lctFlag) {
                this._lct = this._readColorTable(this._lctSize); // read table
                this._act = this._lct; // make local table active
            }
            else {
                this._act = this._gct; // make global table active
                if (this._bgIndex == this._transIndex) {
                    this._bgColor = 0;
                }
            }

            int save = 0;
            if (this._transparency) {
                save = this._act[this._transIndex];
                this._act[this._transIndex] = 0; // set transparent color if specified
            }

            if (this._act == null) {
                this._status = STATUS_FORMAT_ERROR; // no color table defined
            }

            if (this._error()) {
                return;
            }

            this._decodeImageData(); // decode pixel data
            this._skip();

            if (this._error()) {
                return;
            }

            // create new image to receive frame data
            //		image =
            //			new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB_PRE);

            this._image = this._image ?? new int[this._width * this._height];

            this._setPixels(); // transfer pixel data to image

            Buffer.BlockCopy(this._image, 0, this._currentFrame.bytes, 0, this._currentFrame.bytes.Length);
            this._currentFrame.delay = this._delay;
            this._frameCount++;

            if (this._transparency) {
                this._act[this._transIndex] = save;
            }

            this._resetFrame();
        }

        /**
         * Reads Logical Screen Descriptor
         */
        void _readLSD() {
            // logical screen size
            this._width = this._readShort();
            this._height = this._readShort();

            // packed fields
            int packed = this._read();
            this._gctFlag = (packed & 0x80) != 0; // 1   : global color table flag
            // 2-4 : color resolution
            // 5   : gct sort flag
            this._gctSize = 2 << (packed & 7); // 6-8 : gct size

            this._bgIndex = this._read(); // background color index
            this._pixelAspect = this._read(); // pixel aspect ratio
        }

        /**
         * Reads Netscape extenstion to obtain iteration count
         */
        void _readNetscapeExt() {
            do {
                this._readBlock();
                if (this._block[0] == 1) {
                    // loop count sub-block
                    int b1 = this._block[1] & 0xff;
                    int b2 = this._block[2] & 0xff;
                    this._loopCount = (b2 << 8) | b1;
                }
            } while (this._blockSize > 0 && !this._error());
        }

        /**
         * Reads next 16-bit value, LSB first
         */
        int _readShort() {
            // read 16-bit value, LSB first
            return this._read() | (this._read() << 8);
        }

        /**
         * Resets frame state for reading next image.
         */
        void _resetFrame() {
            this._lastDispose = this._dispose;
            this._lix = this._ix;
            this._liy = this._iy;
            this._liw = this._iw;
            this._lih = this._ih;
            this._lastBgColor = this._bgColor;
            this._lct = null;
        }

        /**
         * Skips variable length blocks up to and including
         * next zero length block.
         */
        void _skip() {
            do {
                this._readBlock();
            } while ((this._blockSize > 0) && !this._error());
        }
    }
}