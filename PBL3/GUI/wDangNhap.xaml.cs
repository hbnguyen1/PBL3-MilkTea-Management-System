using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PBL3.Manangers;
using PBL3.Models;

namespace PBL3.GUI
{
    public partial class wDangNhap : Window
    {
        private bool isPasswordVisible = false;

        public wDangNhap()
        {
            InitializeComponent();
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

            // Gọi logic kiểm tra đăng nhập
            var currentUser = AuthManager.Login(phoneNumber, password);

            if (currentUser != null)
            {
                if (currentUser is Staff currentStaff)
                {
                    wTrangChu_NhanVien staffWindow = new wTrangChu_NhanVien();
                    staffWindow.Show();
                    this.Close();
                }
                else if (currentUser is Users currentCustomer)
                {
                    wTrangChu customerWindow = new wTrangChu();
                    customerWindow.Show();
                    this.Close();
                }
            }
        }

        private void lblDangKy_Click(object sender, MouseButtonEventArgs e)
        {
            wDangKy registerWindow = new wDangKy();
            registerWindow.Show();
            this.Close();
        }
    }
}