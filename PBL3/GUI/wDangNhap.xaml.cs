using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PBL3.Core;
using PBL3.Service;
using PBL3.Models;
using PBL3.Interface;
using Microsoft.Extensions.DependencyInjection; // BẮT BUỘC THÊM ĐỂ GỌI FORM TỪ DI

namespace PBL3.GUI
{
    public partial class wDangNhap : Window
    {
        private bool isPasswordVisible = false;

        // 1. Khai báo biến readonly để hứng Service
        private readonly IPasswordAuthenticator _authService;

        // 2. Yêu cầu truyền Service vào Constructor
        public wDangNhap(IPasswordAuthenticator authService)
        {
            InitializeComponent();
            _authService = authService; 
        }

        private void btnTogglePassword_Click(object sender, MouseButtonEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                btnTogglePassword.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F28500"));
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnTogglePassword.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A0A0A0"));
            }
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            string phoneNumber = txtPhoneNumber.Text;
            string password = isPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;
            var currentUser = _authService.Authenticate(phoneNumber, password);

            if (currentUser != null)
            {
                if (currentUser is Staff currentStaff)
                {
                    if (currentStaff.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID nhân viên không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserSession.CurrentUser = currentStaff;
                    var staffWindow = Program.ServiceProvider.GetRequiredService<wTrangChu_NhanVien>();
                    staffWindow.Show();
                    this.Close();
                }
                else if (currentUser is Admin currentAdmin)
                {
                    if (currentAdmin.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID quản trị viên không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserSession.CurrentUser = currentAdmin;

                    var adminWindow = Program.ServiceProvider.GetRequiredService<wTrangChu_Boss>();
                    adminWindow.Show();
                    this.Close();
                }
                else if (currentUser is Users currentCustomer)
                {
                    if (currentCustomer.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID khách hàng không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserSession.CurrentUser = currentCustomer;

                    // (Giữ nguyên dùng new vì wTrangChu đang yêu cầu truyền tham số ID)
                    wTrangChu customerWindow = new wTrangChu(currentCustomer.userID);
                    customerWindow.Show();
                    this.Close();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Số điện thoại hoặc mật khẩu không chính xác!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lblDangKy_Click(object sender, MouseButtonEventArgs e)
        {
            wDangKy registerWindow = Program.ServiceProvider.GetRequiredService<wDangKy>();
            registerWindow.Show();
            this.Close();
        }
    }
}