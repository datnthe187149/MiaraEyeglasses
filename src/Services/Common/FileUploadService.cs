using System.Text.RegularExpressions;

namespace MiaraEyeglasses.Services.Common
{
    // Giao diện xử lý tải tệp ảnh lên máy chủ
    public interface IFileUploadService
    {
        // Lưu tệp ảnh từ form upload của người dùng
        Task<string> SaveImageAsync(IFormFile file, string subFolder = "");

        // Lưu danh sách nhiều tệp ảnh cùng lúc
        Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files, string subFolder = "");

        // Sao chép ảnh từ một đường dẫn có sẵn trên ổ đĩa vào thư mục img
        Task<string> SaveImageFromPathAsync(string sourceFilePath, string subFolder = "");

        // Tự động tạo tên tệp duy nhất để không bị trùng lặp khi nhiều người cùng upload
        string GenerateUniqueFileName(string originalFileName);
    }

    // Lớp triển khai dịch vụ lưu ảnh vào thư mục wwwroot/img
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Tạo tên tệp độc nhất: {tên_gốc}_{thời_gian}_{chuỗi_ngẫu_nhiên}.{đuôi_ảnh}
        public string GenerateUniqueFileName(string originalFileName)
        {
            // Bước 1: Lấy tên file bỏ đuôi mở rộng và lọc bỏ ký tự đặc biệt
            string rawName = Path.GetFileNameWithoutExtension(originalFileName);
            string safeName = Regex.Replace(rawName, @"[^a-zA-Z0-9_-]", "");
            
            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "img";
            }
            if (safeName.Length > 25)
            {
                // Cắt lấy 25 ký tự đầu nếu tên quá dài
                safeName = safeName.Substring(0, 25);
            }

            // Bước 2: Lấy đuôi mở rộng của tệp (ví dụ .jpg, .png, .webp)
            string ext = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(ext))
            {
                ext = ".jpg";
            }

            // Bước 3: Ghép mốc thời gian chi tiết đến mili-giây và 4 ký tự ngẫu nhiên
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string fullGuid = Guid.NewGuid().ToString("N");
            string randomHex = fullGuid.Substring(0, 4);

            return $"{safeName}_{timestamp}_{randomHex}{ext}".ToLower();
        }

        // Lưu 1 tệp ảnh được gửi lên từ trình duyệt vào thư mục wwwroot/img
        public async Task<string> SaveImageAsync(IFormFile file, string subFolder = "")
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            // Xác định thư mục đích trên ổ cứng
            string targetDir;
            if (string.IsNullOrWhiteSpace(subFolder))
            {
                targetDir = Path.Combine(_env.WebRootPath, "img");
            }
            else
            {
                targetDir = Path.Combine(_env.WebRootPath, "img", subFolder);
            }

            // Tự động tạo thư mục nếu trên máy chủ chưa có
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Sinh tên độc nhất và đường dẫn đầy đủ
            string uniqueName = GenerateUniqueFileName(file.FileName);
            string destinationPath = Path.Combine(targetDir, uniqueName);

            // Ghi nội dung tệp vào ổ đĩa bằng FileStream
            using (var stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối để lưu vào cơ sở dữ liệu và hiển thị trên Web
            string relUrl;
            if (string.IsNullOrWhiteSpace(subFolder))
            {
                relUrl = "/img/" + uniqueName;
            }
            else
            {
                relUrl = "/img/" + subFolder + "/" + uniqueName;
            }

            return relUrl.Replace("//", "/");
        }

        // Lưu danh sách nhiều tệp ảnh
        public async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files, string subFolder = "")
        {
            var results = new List<string>();
            if (files == null)
            {
                return results;
            }

            foreach (var f in files)
            {
                if (f != null && f.Length > 0)
                {
                    string path = await SaveImageAsync(f, subFolder);
                    if (!string.IsNullOrEmpty(path))
                    {
                        results.Add(path);
                    }
                }
            }
            return results;
        }

        // Sao chép tệp ảnh từ một đường dẫn có sẵn vào wwwroot/img
        public async Task<string> SaveImageFromPathAsync(string sourceFilePath, string subFolder = "")
        {
            if (!File.Exists(sourceFilePath))
            {
                return string.Empty;
            }

            string targetDir;
            if (string.IsNullOrWhiteSpace(subFolder))
            {
                targetDir = Path.Combine(_env.WebRootPath, "img");
            }
            else
            {
                targetDir = Path.Combine(_env.WebRootPath, "img", subFolder);
            }

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string uniqueName = GenerateUniqueFileName(Path.GetFileName(sourceFilePath));
            string destinationPath = Path.Combine(targetDir, uniqueName);

            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            string relUrl;
            if (string.IsNullOrWhiteSpace(subFolder))
            {
                relUrl = "/img/" + uniqueName;
            }
            else
            {
                relUrl = "/img/" + subFolder + "/" + uniqueName;
            }

            return relUrl.Replace("//", "/");
        }
    }
}
