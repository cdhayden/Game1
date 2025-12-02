using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    /// <summary>
    /// defines a struct for storing screen dimensions conveniently
    /// </summary>
    public struct ScreenDimensions
    {
        public int Left;
        public int Right => Left + Width;
        public int Top;
        public int Bottom => Top + Height;

        public int Height;
        public int Width;
    }
}
