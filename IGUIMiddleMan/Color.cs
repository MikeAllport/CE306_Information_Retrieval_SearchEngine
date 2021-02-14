using System;
using System.Collections.Generic;
using System.Text;

namespace IGUIAdapter
{
    public struct GUIColor
    {
        public static readonly GUIColor ERROR_COLOR = new GUIColor(150, 50, 50);
        public int R { get;  }
        public int G { get;  }
        public int B { get;  }

        public GUIColor(int r, int g, int b)
        {
            if (r > 255 || g > 255 || b > 255)
                throw new Exception($"Error creating Color with values R: {r} G: {g} B: {b}");
            this.R = r;
            this.G = g;
            this.B = b;
        }
    }
}
