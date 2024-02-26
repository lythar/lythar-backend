using LytharBackend.Exceptons;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace LytharBackend.ImageGeneration;

public static class IconCreator
{
    public static async Task<Stream> Generate(Stream input, int targetWidth, int targetHeight)
    {
        try
        {
            using var image = await Image.LoadAsync(input);

            if (image.Width != image.Height)
            {
                var min = Math.Min(image.Width, image.Height);
                var x = (image.Width - min) / 2;
                var y = (image.Height - min) / 2;

                image.Mutate(img => img.Crop(new Rectangle(x, y, min, min)));
            }

            if (image.Width > targetWidth || image.Height > targetHeight)
            {
                image.Mutate(x => x.Resize(targetWidth, targetHeight));
            }

            var memoryStream = new MemoryStream();
            await image.SaveAsWebpAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
        catch (ImageFormatException)
        {
            throw new InvalidImageException();
        }
    }
}
