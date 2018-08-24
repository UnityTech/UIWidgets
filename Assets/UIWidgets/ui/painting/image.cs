using System;
using System.Collections.Generic;
using UIWidgets.painting;
using UnityEngine;

namespace UIWidgets.ui
{
    public class Image
    {
        public Image(byte[] raw, int height = 100, int width = 100)
        {
            this.rawData = raw;
            this.height = height;
            this.width = width;
        }

        public byte[] rawData; // todo temp hack
        public int height; //有别的用吗
        public int width;
    }
}