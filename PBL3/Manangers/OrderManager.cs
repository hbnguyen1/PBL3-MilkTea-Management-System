using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using PBL3.Interface;
using PBL3.Core;

namespace PBL3.Manangers
{
    internal class OrderManager
    {
        public void Order(Users currentUser)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Logger.Info($"Người dùng {currentUser.userID} mở menu đặt món");

            ItemManager itemManager = new ItemManager();
            ItemService itemService = new ItemService();
            OrderService orderService = new OrderService();
            IngredientService ingredientService = new IngredientService();

            itemManager.ShowMenu();

            List<OrderDetails> listorder = new List<OrderDetails>();

            while (true)
            {
                Console.WriteLine("Nhập id món muốn đặt hoặc nhập '0' để thoát");
                string id = Console.ReadLine();

                if (id == "0")
                {
                    Logger.Info($"Người dùng {currentUser.userID} thoát khỏi chức năng đặt món");
                    break;
                }

                if (int.TryParse(id, out int itemId))
                {
                    Logger.Info($"Người dùng {currentUser.userID} chọn món ID {itemId}");

                    itemManager.ShowItemDetails(itemId);

                    Console.WriteLine("Nhập size bạn muốn đặt:");
                    string size = Console.ReadLine().ToUpper();

                    Item? item = itemService.GetItemSize(itemId, size);

                    if (item != null)
                    {
                        Console.WriteLine("Nhập số lượng bạn muốn đặt:");
                        int quantity = int.Parse(Console.ReadLine());

                        Console.WriteLine("Bạn có ghi chú gì không:");
                        string note = Console.ReadLine();

                        Console.WriteLine("Bạn có muốn đặt món này không? (y/n)");
                        string choice = Console.ReadLine();

                        if (choice.ToLower() == "y")
                        {
                            Logger.Info($"Người dùng {currentUser.userID} đặt món {itemId} size {size} số lượng {quantity}");

                            if (ingredientService.CheckIngredientEnough(itemId, size, quantity))
                            {
                                OrderDetails orderDetails = new OrderDetails
                                {
                                    itemID = itemId,
                                    size = size,
                                    quantity = quantity,
                                    priceAtOrder = item.price,
                                    note = note
                                };

                                listorder.Add(orderDetails);

                                Logger.Info($"Đã thêm món {itemId} vào danh sách đặt");

                                Console.WriteLine("Đặt món thành công!");
                            }
                            else
                            {
                                Logger.Warning($"Không đủ nguyên liệu để làm món {itemId}");

                                Console.WriteLine("Không đủ nguyên liệu để làm món này!");
                            }
                        }
                        else
                        {
                            Logger.Info($"Người dùng {currentUser.userID} hủy đặt món {itemId}");
                            Console.WriteLine("Đã hủy đặt món.");
                        }
                    }
                    else
                    {
                        Logger.Warning($"Không tìm thấy món {itemId} size {size}");
                        Console.WriteLine("Không tìm thấy món. Vui lòng thử lại.");
                    }
                }
                else
                {
                    Logger.Warning($"Người dùng nhập ID không hợp lệ: {id}");
                    Console.WriteLine("ID không hợp lệ.");
                }
            }

            if (listorder.Count == 0)
            {
                Logger.Info($"Người dùng {currentUser.userID} chưa đặt món nào");
                Console.WriteLine("Bạn chưa đặt món nào!");
                return;
            }

            int totalprice = 0;

            foreach (var item in listorder)
            {
                totalprice += item.priceAtOrder * item.quantity;
            }

            Console.WriteLine($"Tổng tiền của quý khách là {totalprice} VND, xác nhận đơn hàng? (y/n)");
            string yn = Console.ReadLine().ToLower();

            if (yn == "y")
            {
                Orders order = new Orders()
                {
                    customerID = currentUser.userID,
                    staffID = null,
                    orderDate = DateTime.Now,
                    orderStatus = "Pending",
                    totalPrice = totalprice
                };

                Logger.Info($"Đang tạo đơn hàng cho khách {currentUser.userID} với tổng tiền {totalprice}");

                bool isSuccess = orderService.CreateOrder(order, listorder);

                if (isSuccess)
                {
                    Logger.Info($"Tạo đơn hàng thành công cho khách {currentUser.userID}");

                    PrintBill(order, listorder);
                }
                else
                {
                    Logger.Error($"Lỗi khi tạo đơn hàng cho khách {currentUser.userID}");

                    Console.WriteLine("Hệ thống gặp lỗi trong lúc thanh toán. Đơn hàng đã bị hủy!");
                }
            }
        }

        void PrintBill(Orders order, List<OrderDetails> list)
        {
            Logger.Info($"Đang in hóa đơn cho khách {order.customerID}");

            Console.WriteLine("\n======= HOA DON =======");

            int total = 0;

            foreach (var item in list)
            {
                int money = item.priceAtOrder * item.quantity;

                Console.WriteLine(
                    $"Mon ID: {item.itemID} | Size: {item.size} | SL: {item.quantity} | Gia: {money}"
                );

                total += money;
            }

            Console.WriteLine("------------------------");
            Console.WriteLine($"Tong tien: {total} VND");
            Console.WriteLine($"Ngay: {order.orderDate}");
            Console.WriteLine("========================");
        }
    }
}

