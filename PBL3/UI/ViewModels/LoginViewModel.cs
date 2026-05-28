using PBL3.src.Application;
using PBL3.src.Application.Interface;
using PBL3.src.Domain.Models;
using PBL3.UI.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PBL3.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IPasswordAuthenticator _authenticator;
        private string _phoneNumber;
        public string phoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        public ICommand DangNhapCommand { get; }
        // Sự kiện thông báo ra bên ngoài khi đăng nhập thành công để UI tự chuyển trang
        public event Action<Users> OnLoginSuccess;
        public LoginViewModel(IPasswordAuthenticator authenticator)
        {
            _authenticator = authenticator;
            DangNhapCommand = new RelayCommand(ExecuteDangNhap, CanExecuteDangNhap);
        }
        private bool CanExecuteDangNhap(object obj)
        {
            return !string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(Password);
        }
        private void ExecuteDangNhap(object obj)
        {
            var currentUser = _authenticator.Authenticate(phoneNumber, Password);
            if (currentUser != null)
            {
                if (currentUser is Staff currentStaff)
                {
                    if (currentStaff.isAvailable == true)
                    {
                        if (currentStaff.userID <= 1)
                        {
                            System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        UserSession.CurrentUser = currentStaff;
                        OnLoginSuccess?.Invoke(currentUser);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else if (currentUser is Admin currentAdmin)
                {
                    if (currentAdmin.userID <= 1)
                    {
                        System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserSession.CurrentUser = currentAdmin;
                    OnLoginSuccess?.Invoke(currentAdmin);
                }
                else if (currentUser is Customer currentCustomer)
                {
                    if (currentCustomer.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserSession.CurrentUser = currentCustomer;
                    OnLoginSuccess?.Invoke(currentCustomer);
                }
                else
                {
                    System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
