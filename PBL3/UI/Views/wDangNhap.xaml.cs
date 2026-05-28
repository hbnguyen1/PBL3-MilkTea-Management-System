using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Domain.Models;
using PBL3.src.Application.Interface;
using PBL3.src.Application;
using PBL3.UI.ViewModels;

namespace PBL3.UI.Views
{
    public partial class wDangNhap : Window
    {
        private bool isPasswordVisible = false;
        private readonly LoginViewModel _viewModel;
        public wDangNhap(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel; 
            this.DataContext = _viewModel; // Khởi tạo liên kết dữ liệu để data binding
            _viewModel.OnLoginSuccess += ViewModel_OnLoginSuccess;
        }
        private void ViewModel_OnLoginSuccess(Users user)
        {
            if (user is Staff)
            {
                var staffWindow = Program.ServiceProvider.GetRequiredService<wTrangChu_NhanVien>();
                staffWindow.Show();
            }
            else if (user is Admin)
            {
                var adminWindow = Program.ServiceProvider.GetRequiredService<wTrangChu_Boss>();
                adminWindow.Show();
            }
            else
            {
                wTrangChu customerWindow = new wTrangChu(user.userID);
                customerWindow.Show();
            }
            this.Close();
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

        // Khi gõ vào ô mật khẩu ẩn (dấu chấm đen)
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible && _viewModel != null)
            {
                _viewModel.Password = txtPassword.Password;
            }
        }

        // Khi gõ vào ô mật khẩu hiện (chữ thường)
        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isPasswordVisible && _viewModel != null)
            {
                _viewModel.Password = txtPasswordVisible.Text;
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