using PBL3.Interface;
using System;
using PBL3.Manangers;
using PBL3.Models;
using PBL3.Core;
using System.ComponentModel;
namespace PBL3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            RecipeManager recipeManager = new RecipeManager();
            ItemManager itemManager = new ItemManager();
            IngredientManager ingredientManager = new IngredientManager();
            OrderManager orderManager = new OrderManager();
            CustomerManagers customerManagers = new CustomerManagers();
            
            while (true)
            {
                Console.WriteLine("Chào mừng đến với hệ thống quản lý trà sữa!");
                Console.WriteLine("1. Đăng ký tài khoản");
                Console.WriteLine("2. Đăng nhập");
                Console.WriteLine("3. Thoát");
                Console.Write("Vui lòng chọn một tùy chọn: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        CustomerManagers.Register();
                        break;
                    case "2":
                        AuthManager.Login();
                        if (UserSession.CurrentUser is Customer)
                        {
                            Console.WriteLine("Bảng chọn khách hàng:");
                            Console.WriteLine("1. Xem menu");
                            Console.WriteLine("2. Đặt hàng");
                            string choice2 = Console.ReadLine();
                            switch (choice2)
                            {
                                case "1":
                                    itemManager.ShowMenu();
                                    break;
                                case "2":
                                    orderManager.Order(UserSession.CurrentUser);
                                    break;
                                default:
                                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                                    break;
                            }
                        }
                        if (UserSession.CurrentUser is Staff)
                        {
                            Console.WriteLine("Bảng chọn nhân viên:");
                            Console.WriteLine("1. Quản lý đơn hàng");
                            Console.WriteLine("2. Quản lý sản phẩm");
                        }
                        break;
                    case "3":
                        Console.WriteLine("Cảm ơn bạn đã sử dụng hệ thống. Tạm biệt!");
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                        break;
                }
            }
        }
    }
}
