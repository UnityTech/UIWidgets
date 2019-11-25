using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.editor {
    public class WindowConfig {
        public readonly bool disableRasterCache;

#if UNITY_ANDROID
            //make API compatible to low-end Android devices
            public static float MaxRasterImageSize = 2048;
#else
            public static float MaxRasterImageSize = 4096;
#endif

        static bool? _disableComputeBuffer = null;

        public static bool disableComputeBuffer {
            //disable compute buffer by default for now
            get { return _disableComputeBuffer ?? true; }
            set {
                D.assert(_disableComputeBuffer == null
                    || _disableComputeBuffer == value
                    , () => "The global settings of [disableComputeBuffer] cannot be initiated for multiple times!");

                _disableComputeBuffer = value;
            }
        }

        public WindowConfig(bool disableRasterCache) {
            this.disableRasterCache = disableRasterCache;
        }

        public static readonly WindowConfig defaultConfig = new WindowConfig(
            disableRasterCache: false
        );
    }
}