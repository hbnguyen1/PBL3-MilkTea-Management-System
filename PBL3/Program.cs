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
            ImportManager importManager = new ImportManager();
            OrderProcessingManager orderProcessingManager = new OrderProcessingManager();
            StaffManager staffManager = new StaffManager();
            ReportManager reportManager = new ReportManager();

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
                        string name = Console.ReadLine();
                        string phoneNumber = Console.ReadLine();
                        string password = Console.ReadLine();
                        CustomerManagers.Register(name, phoneNumber, password);
                        break;
                    case "2":
                        AuthManager.Login();
                        if (UserSession.CurrentUser is Admin)
                        {
                        Console.WriteLine("Bảng chọn quản trị viên:");
                        Console.WriteLine("1. Quản lý nhân viên");
                        Console.WriteLine("2. Xem báo cáo");
                        string choice4 = Console.ReadLine();
                        switch (choice4)
                            {
                            case "1":
                                //staffManager.ManageStaff();
                                //throw Exceptions.NotImplementedException();
                                Console.WriteLine("Chua xong shop oi");
                                break;
                            case "2":
                                reportManager.ShowTopSellingReport();
                                break;
                            default:
                                Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                            break;
                            }
                        }
                        if (UserSession.CurrentUser is Customer)
                        {
                            Console.WriteLine("Bảng chọn khách hàng:");
                            Console.WriteLine("1. Xem menu");
                            Console.WriteLine("2. Đặt hàng");
                            Console.WriteLine("3. Xem lịch sử đơn hàng");
                            Console.WriteLine("4. Xem điểm thưởng");
                            Console.WriteLine("5. Xem best seller");
                            string choice2 = Console.ReadLine();
                            switch (choice2)
                            {
                                case "1":
                                    itemManager.ShowMenu(true);
                                    break;
                                case "2":
                                    orderManager.Order(UserSession.CurrentUser);
                                    break;
                                case "3":
                                    throw new NotImplementedException();
                                    break;
                                case "4":
                                    throw new NotImplementedException();
                                case "5":
                                    customerManagers.ShowBestSeller();
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
                            Console.WriteLine("2. Quản lý kho");
                            Console.WriteLine("3. Quản lý món");
                            Console.WriteLine("4. Check-in / Check-out");
                            Console.WriteLine("5. Đăng ký ca làm tuần");
                            Console.WriteLine("6. Xem lương");
                            Console.WriteLine("7. Chốt lương tháng");
                            Console.WriteLine("8. Đặt cho khách hàng"); 
                            string choice3 = Console.ReadLine();
                            switch (choice3)
                            {
                                case "1":
                                    orderProcessingManager.ShowAndApprovePendingOrders(UserSession.CurrentUser);
                                    break;
                                case "2":
                                    importManager.Import(UserSession.CurrentUser.userID);
                                    break;
                                case "3":
                                    Console.WriteLine("Bạn có muốn thêm món không ? (y/n)");
                                    if (Console.ReadLine().ToLower() == "y")
                                    {
                                        itemManager.AddItem();
                                    }
                                    Console.WriteLine("Bạn có muốn xóa món không ? (y/n)");
                                    if (Console.ReadLine().ToLower() == "y")
                                    {   
                                        itemManager.ShowMenu(false);
                                        int id = int.Parse(Console.ReadLine());
                                        itemManager.DeleteItemByID(id);
                                    }
                                    break;
                                case "4":
                                    staffManager.ToggleShift(UserSession.CurrentUser.userID);
                                    break;
                                case "5":
                                    staffManager.RegisterWeeklySchedule(UserSession.CurrentUser.userID);
                                    break;
                                case "6":
                                    Console.Write("Nhập tháng: ");
                                    int m = int.Parse(Console.ReadLine());

                                    Console.Write("Nhập năm: ");
                                    int y = int.Parse(Console.ReadLine());

                                    double salary = staffManager.CalculateSalary(UserSession.CurrentUser.userID, m, y);

                                    Console.WriteLine($"Lương tháng {m}/{y}: {salary}");
                                    break;
                                case "7":
                                    Console.Write("Nhập tháng: ");
                                    int m2 = int.Parse(Console.ReadLine());

                                    Console.Write("Nhập năm: ");
                                    int y2 = int.Parse(Console.ReadLine());

                                    staffManager.SaveSalary(UserSession.CurrentUser.userID, m2, y2);
                                    break;
                                case "8":
                                    staffManager.StaffCreateOrderForCustomer();
                                    break;
                            }
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
