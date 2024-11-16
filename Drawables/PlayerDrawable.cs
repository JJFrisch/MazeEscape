using Microsoft.Maui.Graphics.Platform;
using System;
using IImage = Microsoft.Maui.Graphics.IImage;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MazeEscape.Drawables
{

    internal class PlayerDrawable : BindableObject, IDrawable
    {
        IImage image;

        public int PlayerX;
        public int PlayerY;
        public int Moves;
        public double WindowWidth;
        public double WindowHeight;
        public int MazeWidth;
        public int MazeHeight;
        //PlatformImage image;
        public void Initialize()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("MazeEscape.Resources.Images.player_image.png"))
            {
                image = PlatformImage.FromStream(stream);
            }
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var cell_width = (float)(WindowWidth / MazeWidth);
            var cell_height = (float)(WindowHeight / MazeHeight);
            var x_pos = (float)(PlayerX * cell_width);
            var y_pos = (float)(PlayerY * cell_height);
            float scale = 1;

            if (image.Width > cell_width || image.Height > cell_height)
            {
                scale = Math.Min(cell_width / image.Width, cell_height / image.Height);
            }

            var xPadding = (cell_width - (image.Width * scale)) / 2;
            var yPadding = (cell_height - (image.Height * scale)) / 2;

            canvas.FontColor = Colors.Blue;
            canvas.FontSize = 18;
            canvas.DrawString(Moves.ToString(), 2000, 20, 50, 50, HorizontalAlignment.Right, VerticalAlignment.Top);

            if (image != null)
            {
                canvas.DrawImage(image, x_pos+xPadding, y_pos+yPadding, (image.Width * scale), (image.Height * scale));
            }

        }


    }
}
