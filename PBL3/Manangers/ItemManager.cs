using Microsoft.EntityFrameworkCore.Query;
using PBL3.Core;
using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq; // Cần thêm thư viện này cho LINQ (FirstOrDefault, Where)
using System.Text;

namespace PBL3.Manangers
{
    internal class ItemManager
    {
        public void AddItem()
        {
            ItemService itemService = new ItemService();
            IngredientService igService = new IngredientService();
            Console.Clear();
            Console.WriteLine("=== THÊM SẢN PHẨM MỚI ===");
            int itemID = itemService.GetNextItemID();
            Console.Write("Nhập tên sản phẩm (VD: Trà sữa Oolong): ");
            string itemName = Console.ReadLine();
            Console.Write("Nhập loại sản phẩm (VD: TraSua, Cafe): ");
            string itemType = Console.ReadLine();
            List<Ingredient> ingredients = igService.GetAllIngredients();
            List<Item> itemsToSave = new List<Item>();
            List<Recipe> recipesToSave = new List<Recipe>();
            List<Recipe> baseRecipes = new List<Recipe>();
            string[] sizes = { "M", "L" };
            foreach (string currentSize in sizes)
            {
                Console.WriteLine($"\n=========================================");
                Console.WriteLine($"--- CẤU HÌNH CHO SIZE {currentSize} ---");
                Console.Write($"Nhập giá bán cho size {currentSize}: ");
                int price = int.Parse(Console.ReadLine());
                itemsToSave.Add(new Item
                {
                    itemID = itemID,
                    size = currentSize,
                    itemName = itemName,
                    itemType = itemType,
                    isAvailable = true,
                    price = price
                });
                Console.WriteLine($"\n[Nhập công thức pha chế Size {currentSize}]");
                if (currentSize == sizes[0])
                {
                    while (true)
                    {
                        Console.Write($"\nNhập 'ID' hoặc 'Tên nguyên liệu' (Hoặc gõ '0' để chốt công thức size {currentSize}): ");
                        string input = Console.ReadLine().Trim();

                        if (input == "0") break;

                        Ingredient foundIngredient = null;
                        if (int.TryParse(input, out int igId))
                        {
                            foundIngredient = ingredients.FirstOrDefault(i => i.igID == igId);
                        }
                        else
                        {
                            var matches = ingredients.Where(i => i.igName.ToLower().Contains(input.ToLower())).ToList();

                            if (matches.Count == 1)
                            {
                                foundIngredient = matches[0];
                                Console.WriteLine($"Tìm thấy nguyên liệu: {foundIngredient.igName}");
                            }
                            else if (matches.Count > 1)
                            {
                                Console.WriteLine($"\nTìm thấy {matches.Count} nguyên liệu chứa từ khóa '{input}':");
                                foreach (var m in matches)
                                {
                                    Console.WriteLine($"    -> ID: {m.igID,-5} | Tên: {m.igName}");
                                }
                                Console.WriteLine("=> Vui lòng gõ chính xác ID của nguyên liệu bạn muốn chọn!");
                                continue;
                            }
                        }

                        if (foundIngredient != null)
                        {
                            Console.Write($"Nhập định lượng cho [{foundIngredient.igName}] (Ví dụ 20): ");
                            if (int.TryParse(Console.ReadLine(), out int quantity))
                            {
                                Console.Write($"Nhập đơn vị cho [{foundIngredient.igName}] (Ví dụ g/ml): ");
                                string unitUsed = Console.ReadLine();

                                if (!string.IsNullOrEmpty(unitUsed))
                                {
                                    Recipe newR = new Recipe
                                    {
                                        itemID = itemID,
                                        size = currentSize,
                                        ingredientID = foundIngredient.igID,
                                        quantityNeeded = quantity,
                                        unitUsed = unitUsed
                                    };

                                    recipesToSave.Add(newR);
                                    baseRecipes.Add(newR);

                                    Console.WriteLine($"  -> Đã thêm {quantity} {unitUsed} {foundIngredient.igName} vào công thức size {currentSize}!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Định lượng phải là số!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Không tìm thấy nguyên liệu nào khớp với từ khóa của bạn!");
                        }
                    }
                }
                else
                {
                    if (baseRecipes.Count > 0)
                    {
                        Console.WriteLine($"Theo công thức từ Size {sizes[0]}. Vui lòng nhập định lượng mới:");

                        foreach (var baseR in baseRecipes)
                        {
                            var ig = ingredients.FirstOrDefault(i => i.igID == baseR.ingredientID);
                            string igName = ig != null ? ig.igName : "Nguyên liệu ẩn";

                            int newQuantity = 0;
                            while (true)
                            {
                                Console.Write($"- [{igName}] (Size {sizes[0]} dùng {baseR.quantityNeeded} {baseR.unitUsed}) -> Định lượng Size {currentSize}: ");
                                if (int.TryParse(Console.ReadLine(), out newQuantity))
                                {
                                    break;
                                }
                                Console.WriteLine("Định lượng phải là số, vui lòng nhập lại!");
                            }

                            recipesToSave.Add(new Recipe
                            {
                                itemID = itemID,
                                size = currentSize,
                                ingredientID = baseR.ingredientID,
                                quantityNeeded = newQuantity,
                                unitUsed = baseR.unitUsed
                            });
                        }
                        Console.WriteLine($"-> Đã thiết lập xong công thức cho Size {currentSize}!");
                    }
                    else
                    {
                        Console.WriteLine($"Size {sizes[0]} không có công thức, hệ thống bỏ qua công thức Size {currentSize}.");
                    }
                }
            }
            if (itemsToSave.Count > 0)
            {
                Console.WriteLine("\nĐang lưu sản phẩm và công thức...");
                if (itemService.AddItemWithRecipe(itemsToSave, recipesToSave))
                {
                    Console.WriteLine("Thêm sản phẩm và công thức hoàn tất 100%!");
                }
                else
                {
                    Console.WriteLine("Thêm sản phẩm thất bại. Vui lòng kiểm tra Log.");
                }
            }
            else
            {
                Console.WriteLine("\nBạn chưa cấu hình Size nào, đã hủy thao tác tạo món.");
            }

            Console.WriteLine("Nhấn phím bất kỳ để tiếp tục...");
            Console.ReadKey();
        }

        public bool DeleteItemByID(int id)
        {
            using (var db = new MilkTeaDBContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var itemsToDelete = db.Items.Where(i => i.itemID == id).ToList();

                        if (itemsToDelete.Count == 0)
                        {
                            return false; 
                        }

                        var recipesToDelete = db.Recipes.Where(r => r.itemID == id).ToList();

                        if (recipesToDelete.Count > 0)
                        {
                            db.Recipes.RemoveRange(recipesToDelete);
                        }
                        db.Items.RemoveRange(itemsToDelete);
                        db.SaveChanges();
                        transaction.Commit();

                        Console.WriteLine($"Xóa món có id : {id} thành công");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Logger.Error($"Lỗi khi xóa món {id}: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Logger.Error("Chi tiết SQL: " + ex.InnerException.Message);
                        }

                        return false;
                    }
                }
            }
        }

        public Item? GetItem()
        {
            Console.WriteLine("Nhap id muon lay: ");
            int id = int.Parse(Console.ReadLine());
            ItemService itemService = new ItemService();
            var item = itemService.GetItemById(id);

            if (item != null)
            {
                return item;
            }
            else
            {
                Console.WriteLine("Không tìm thấy sản phẩm với id đã nhập!");
                return null;
            }
        }

        public void UpdateItem()
        {
            // throw Exception("Chưa hoàn thiện chức năng này, vui lòng quay lại sau!");
        }

        public void ShowMenu(bool showDetail = false)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ItemService itemService = new ItemService();

            var MenuMilkTea = itemService.GetMenuByCategory("Milk Tea");
            var MenuFruitTea = itemService.GetMenuByCategory("Fruit Tea");
            var MenuTopping = itemService.GetMenuByCategory("Topping");

            Console.WriteLine("===== MENU =====");

            foreach (var it in MenuMilkTea)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }

            foreach (var it in MenuFruitTea)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }

            foreach (var it in MenuTopping)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }

            if (showDetail)
            {
                Console.WriteLine("Nhập id sản phẩm muốn xem chi tiết:");
                int id = int.Parse(Console.ReadLine());
                ShowItemDetails(id);
            }
        }

        public void ShowItemDetails(int id)
        {
            ItemService itemService = new ItemService();
            var itemDetails = itemService.GetItemSizeAndPrice(id);

            foreach (var it in itemDetails)
            {
                Console.WriteLine($"{it.itemName} : {it.size} : {it.price} : {it.isAvailable}");
            }
        }
    }
}