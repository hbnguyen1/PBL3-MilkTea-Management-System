using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;

namespace PBL3.src.Domain.Models
{
    public class Item
    {
        public int itemID { get; set; }
        public required string size { get; set; } = string.Empty;
        public required string itemName { get; set; } = string.Empty;
        public required string itemType { get; set; } = string.Empty;
        public Boolean isAvailable { get; set; } = true;
        public required double price { get; set; } = 0;
        public string? ImagePath { get; set; }
        [NotMapped]
        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath)) return null;

                // 1. Loại bỏ tiền tố / hoặc \ đầu tiên nếu có để tránh lỗi Combine
                string cleanPath = ImagePath.Replace("/", "\\").TrimStart('\\');

                // 2. Lấy đường dẫn thư mục gốc của ứng dụng
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 3. Kết hợp và lấy đường dẫn tuyệt đối
                string fullPath = Path.Combine(baseDirectory, cleanPath);

                // 4. KIỂM TRA: Nếu file tồn tại ở bin\Debug thì trả về, 
                // nếu không thì thử tìm trong thư mục gốc của project (nếu đang debug)
                if (File.Exists(fullPath)) return fullPath;

                // Trường hợp ảnh ở trong Solution (khi đang phát triển)
                string projectPath = Path.Combine(Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName, cleanPath);
                return File.Exists(projectPath) ? projectPath : null;
            }
        }
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    }
}
