using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using PBL3.Interface;
namespace PBL3.Manangers
{
    internal class OrderManager
    {
        public void Order(Users currentUser){
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ItemManager itemManager = new ItemManager();
            ItemService itemService = new ItemService();
            OrderService orderService = new OrderService();
            itemManager.ShowMenu();
            List<OrderDetails> listorder = new List<OrderDetails>();
            while (true)
            {
                Console.WriteLine("Nhập id món muốn đặt hoặc nhập '0' để thoát");
                string id = Console.ReadLine();
                if (id == "0")
                {
                    break;
                }
                if (int.TryParse(id, out int itemId))
                {
                    itemManager.ShowItemDetails(itemId);
                    Console.WriteLine("Nhập size bạn muốn đặt");
                    string size = Console.ReadLine();
                    size = size.ToUpper();
                    Item? item = itemService.GetItemSize(itemId, size);
                    if (item != null)
                    {
                        Console.WriteLine("Nhập số lượng bạn muốn đặt");
                        int quantity = int.Parse(Console.ReadLine());
                        Console.WriteLine("Bạn có ghi chú gì không");
                        string note = Console.ReadLine();
                        Console.WriteLine("Bạn có muốn đặt món này không? (y/n)");
                        string choice = Console.ReadLine();
                        if (choice.ToLower() == "y")
                        {
                            if (itemService.isAvailableWithCount(itemId, size, quantity))
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
                                    Console.WriteLine("Đặt món thành công!");
                                }
                            else
                            {
                                Console.WriteLine("Món không có sẵn");
                            } 
                        }
                        else
                        {
                            Console.WriteLine("Đã hủy đặt món.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Item not found. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid item ID.");
                }
            }
            int totalprice = 0;
            foreach (var item in listorder)
            {
                totalprice += (item.priceAtOrder * item.quantity);
            }
            Console.WriteLine($"Tổng tiền của quý khách là {totalprice}VND, mời quý khách xác nhận đơn hàng (y/n)");
            string yn = Console.ReadLine();
            yn = yn.ToLower();
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

                bool isSuccess = orderService.CreateOrder(order, listorder);

                if (isSuccess)
                {
                    Console.WriteLine("Thanh toán thành công! Cảm ơn quý khách.");
                }
                else
                {
                    Console.WriteLine("Hệ thống gặp lỗi trong lúc thanh toán. Đơn hàng đã bị hủy!");
                }
            }
        }
    }
}
