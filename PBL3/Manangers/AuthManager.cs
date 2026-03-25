
using PBL3.Core;
using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Manangers
{
    internal class AuthManager
    {
        public static void Login()
        {
            Console.WriteLine("Đăng nhập vào hệ thống:");
            string phoneNumber = Console.ReadLine();
            string password = Console.ReadLine();
            UserAuthenticator authenticator = new UserAuthenticator();
            var currenuser = authenticator.Authenticate(phoneNumber, password);
            if (currenuser != null)
            {   
                UserSession.CurrentUser = currenuser;
                if (currenuser is Users)
                {
                    Console.WriteLine("Chào mừng khách hàng đăng nhập thành công!");
                    Console.WriteLine($"Mã khách hàng KH{currenuser.userID}");
                    Logger.Info($"Khách hàng {currenuser.userID} đăng nhập thành công");
                }
                else if (currenuser is Staff)
                {
                    Console.WriteLine("Chào mừng nhân viên dăng nhập thành công!");
                    Console.WriteLine($"Mã nhân viên NV{currenuser.userID}");
                    Logger.Info($"Nhân viên {currenuser.userID} đăng nhập thành công");
                }
            }
            else
            {
                Console.WriteLine("Đăng nhập thất bại: Tài khoản không tồn tại hoặc sai mật khẩu!");
                Logger.Warning($"Đăng nhập thất bại {phoneNumber}");
            }
        }
    }
}
