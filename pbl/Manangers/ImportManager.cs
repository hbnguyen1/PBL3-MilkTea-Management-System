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
                Console.WriteLine("Nhập ID nguyên liệu (0 để kết thúc):");
                int id = int.Parse(Console.ReadLine());

                if (id == 0) break;

                Console.WriteLine("Nhập số lượng:");
                int quantity = int.Parse(Console.ReadLine());

                Console.WriteLine("Nhập giá nhập:");
                int price = int.Parse(Console.ReadLine());

                details.Add(new ImportDetail
                {
                    igId = id,
                    quantityAdded = quantity,
                    importPrice = price
                });

                Logger.Info($"Thêm nguyên liệu {id} SL {quantity} giá {price} vào phiếu nhập");
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