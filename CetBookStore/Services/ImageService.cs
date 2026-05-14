using System.Drawing;
using System.Drawing.Imaging;

namespace CetBookStore.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _env;
        private const int MAX_WIDTH = 1024;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new Exception("Resim seçilmedi");

            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif")
                throw new Exception("Sadece jpg, png ve gif yüklenebilir");

            if (imageFile.Length > 5 * 1024 * 1024)
                throw new Exception("Dosya 5MB'dan büyük");

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var path = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = imageFile.OpenReadStream())
            {
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }

            ResizeImage(path);
            return fileName;
        }

        private void ResizeImage(string imagePath)
        {
            using (var image = new Bitmap(imagePath))
            {
                if (image.Width > MAX_WIDTH)
                {
                    int newHeight = (int)(image.Height * (MAX_WIDTH / (double)image.Width));
                    using (var resized = new Bitmap(image, new Size(MAX_WIDTH, newHeight)))
                    {
                        resized.Save(imagePath);
                    }
                }
            }
        }

        public void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var path = Path.Combine(_env.WebRootPath, "images", fileName);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }
        }
    }
}
