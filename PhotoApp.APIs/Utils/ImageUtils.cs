using System.Drawing;
using ImageMagick;

namespace PhotoApp.APIs.Utils
{
    public class ImageUtils
    {
        public static MagickGeometry GetThumbnailSize(MagickImage original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 150;

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
