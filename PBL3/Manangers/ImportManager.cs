using System;
using System.Collections.Generic;
using PBL3.Models;
using PBL3.Interface;
using PBL3.Core;

namespace PBL3.Manangers
{
    internal class ImportManager
    {
        public void Import(int staffId)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IngredientService ingredientService = new IngredientService();
            ImportService importService = new ImportService();

            var ingredients = ingredientService.GetAllIngredients();

            Console.WriteLine("===== DANH SÁCH NGUYÊN LIỆU =====");

            foreach (var ig in ingredients)
            {
                Console.WriteLine($"ID: {ig.igID} | {ig.igName} | Tồn: {ig.igCount}");
            }

            List<ImportDetail> details = new List<ImportDetail>();
            while (true)
            {
                Console.WriteLine("\nNhập ID nguyên liệu (0 để kết thúc):");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Lỗi: ID phải là số. Vui lòng nhập lại!");
                    continue; 
                }

                if (id == 0) break;

                //Kiểm tra ID có tồn tại hay không
                var existIg = ingredients.FirstOrDefault(i => i.igID == id);
                if (existIg == null)
                {
                    Console.WriteLine($"Lỗi: Không tìm thấy nguyên liệu nào có ID = {id}!");
                    continue;
                }

                Console.WriteLine($"-> Đang nhập kho cho: [{existIg.igName}]");

                //Nhập số lượng
                Console.WriteLine("Nhập số lượng:");
                int quantity;
                while (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
                {
                    Console.WriteLine("Lỗi: Số lượng phải là số nguyên lớn hơn 0. Nhập lại:");
                }

                //Nhập giá
                Console.WriteLine("Nhập giá nhập (1 đơn vị):");
                int price;
                while (!int.TryParse(Console.ReadLine(), out price) || price < 0)
                {
                    Console.WriteLine("Lỗi: Giá nhập không hợp lệ. Nhập lại:");
                }

                //Xử lý logic cộng dồn nếu nguyên liệu đã tồn tại trong danh sách tạm
                var existingDetail = details.FirstOrDefault(d => d.igId == id);
                if (existingDetail != null)
                {
                    existingDetail.quantityAdded += quantity;
                    existingDetail.importPrice = price; // Cập nhật lại giá mới nhất (hoặc bạn có thể tính trung bình cộng tùy logic)
                    Console.WriteLine($"Đã cộng dồn thêm số lượng cho [{existIg.igName}].");
                }
                else
                {
                    details.Add(new ImportDetail
                    {
                        igId = id,
                        quantityAdded = quantity,
                        importPrice = price
                    });
                    Console.WriteLine($"Đã thêm [{existIg.igName}] vào phiếu nhập.");
                }

                Logger.Info($"Đã thêm/cập nhật nguyên liệu {id} SL {quantity} giá {price} vào danh sách tạm");
            }

            if (details.Count == 0)
            {
                Console.WriteLine("Không có dữ liệu nhập!");
                return;
            }

            bool success = importService.CreateImport(staffId, details);

            if (success)
            {
                Console.WriteLine("Nhập kho thành công!");
            }
            else
            {
                Console.WriteLine("Nhập kho thất bại!");
            }
        }
    }
}