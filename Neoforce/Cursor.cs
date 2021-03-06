﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TomShane.Neoforce.Controls
{
    /// <summary>
    /// Provides a basic Software cursor
    /// </summary>
    public class Cursor
    {
        private Texture2D cursorTexture;

        public Texture2D CursorTexture
        {
            get { return cursorTexture; }
            set { cursorTexture = value; }
        }

        private Vector2 hotspot;
        private int width;
        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public Vector2 HotSpot
        {
            get { return hotspot; }
            set { hotspot = value; }
        }

        public Cursor(Texture2D texture, Vector2 hotspot, int width, int height)
        {
            this.cursorTexture = texture;
            this.hotspot = hotspot;
            this.width = width;
            this.height = height;
        }
    }
}
