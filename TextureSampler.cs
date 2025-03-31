using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon3D
{
    public class TextureSampler
    {
        private Bitmap texture;

        public TextureSampler(string texturePath)
        {
            texture = new Bitmap(texturePath);
        }

        public Color GetPixelColor(double X, double Y)
        {
            int x = (int)(X * (texture.Width - 1));
            int y = (int)(Y * (texture.Height - 1));

            return texture.GetPixel(x, y);
        }
    }
}
