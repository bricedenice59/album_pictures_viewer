using System.Drawing;
using ImageMagick;

namespace PhotoApp.APIs.Utils
{
    public class ImageUtils
    {
        //https://stackoverflow.com/questions/19456036/detect-exif-orientation-and-rotate-image-using-imagemagick
        public static void Autorotate(MagickImage original)
        {
            switch (original.Orientation) {
                case OrientationType.TopLeft:
                break;
                case OrientationType.TopRight:
                    original.Flop();
                    break;
                case OrientationType.BottomRight:
                    original.Rotate( 180);
                break;
                case OrientationType.BottomLeft:
                    original.Flop();
                    original.Rotate(180);
                break;
                case OrientationType.LeftTop:
                    original.Flop();
                    original.Rotate(-90);
                    break;
                case OrientationType.RightTop:
                    original.Rotate(90);
                    break;
                case OrientationType.RightBottom:
                    original.Flop();
                    original.Rotate(90);
                    break;
                case OrientationType.LeftBotom:
                    original.Rotate(-90);
                    break;
                default: // Invalid orientation
                break;
            }
        }

        public static MagickGeometry GetThumbnailSize(MagickImage original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 500;

            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Return original size if image is smaller than maxPixels
            if (originalWidth <= maxPixels || originalHeight <= maxPixels)
            {
                return new MagickGeometry(originalWidth, originalHeight);
            }

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new MagickGeometry((int)(originalWidth * factor), (int)(originalHeight * factor));
        }
    }
}
