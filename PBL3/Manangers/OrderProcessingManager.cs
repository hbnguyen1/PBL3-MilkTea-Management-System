using System;
using System.Collections.Generic;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.Manangers
{
    internal class OrderProcessingManager
    {
        public void ShowAndApprovePendingOrders(Users currentStaff)
        {
            OrderService orderService = new OrderService();
            ItemService itemService = new ItemService();

            while (true)
            {
                List<Orders> pendingList = orderService.GetPendingOrders();

                Console.Clear();
                Console.WriteLine("=== DANH SÁCH ĐƠN HÀNG CHỜ DUYỆT ===");

                if (pendingList.Count == 0)
                {
                    Console.WriteLine("Hiện không có đơn hàng nào chờ duyệt!");
                    Console.WriteLine("Nhấn phím bất kỳ để thoát...");
                    Console.ReadKey();
                    break;
                }

                Console.WriteLine("{0,-10} | {1,-20} | {2,-15}", "Mã Đơn", "Thời gian đặt", "Tổng tiền");
                Console.WriteLine("--------------------------------------------------");
                foreach (var order in pendingList)
                {
                    Console.WriteLine($"{order.orderID,-10} | {order.orderDate.ToString("HH:mm:ss dd/MM"),-20} | {order.totalPrice,-15:N0} VNĐ");
                }

                Console.WriteLine("\nNhập 'Mã Đơn' để xem chi tiết và duyệt (hoặc nhập '0' để quay lại menu chính):");
                string input = Console.ReadLine();

                if (input == "0") break;

                if (int.TryParse(input, out int orderIdToApprove))
                {
                    List<OrderDetails> details = orderService.GetOrderDetails(orderIdToApprove);
                    if (details != null && details.Count > 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"=== CHI TIẾT ĐƠN HÀNG #{orderIdToApprove} ===");
                        Console.WriteLine("{0,-20} | {1,-5} | {2,-10}", "Tên món", "Size", "Số lượng");
                        Console.WriteLine("---------------------------------------------");
                        foreach (var d in details)
                        {
                            Item itemInfo = itemService.GetItemSize(d.itemID, d.size);
                            string itemName = itemInfo != null ? itemInfo.itemName : "Món không xác định";

                            Console.WriteLine($"{itemName,-20} | {d.size,-5} | {d.quantity,-10}");
                            if (!string.IsNullOrEmpty(d.note))
                            {
                                Console.WriteLine($"   * Ghi chú: {d.note}");
                            }
                        }
                        Console.WriteLine("---------------------------------------------");
                        Console.WriteLine("\nBạn có muốn duyệt đơn này không? (y/n)");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            bool isSuccess = orderService.ApproveOrder(orderIdToApprove, currentStaff.userID);
                            if (isSuccess)
                            {
                                Console.WriteLine($"\nĐã duyệt đơn #{orderIdToApprove} thành công! Mời bắt tay vào pha chế.");
                            }
                                
                            else
                                Console.WriteLine("\nDuyệt đơn thất bại. Có thể đơn đã được xử lý bởi người khác.");
                        }
                        else
                        {
                            Console.WriteLine("\nĐã hủy duyệt đơn.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nMã đơn sai hoặc đơn này không có chi tiết món ăn!");
                    }
                }
                else
                {
                    Console.WriteLine("\nMã đơn không hợp lệ!");
                }

                Console.WriteLine("\nNhấn phím bất kỳ để tải lại danh sách...");
                Console.ReadKey();
            }
        }
    }
}