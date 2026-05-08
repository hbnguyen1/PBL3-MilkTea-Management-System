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

            var currentUser = AuthManager.Login(phoneNumber, password);

            if (currentUser != null)
            {
                if (currentUser is Staff currentStaff)
                {
                    if (currentStaff.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID nhân viên không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    wTrangChu_NhanVien staffWindow = new wTrangChu_NhanVien();
                    staffWindow.Show();
                    this.Close();
                }
                else if (currentUser is Users currentCustomer)
                {
                    if (currentCustomer.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID khách hàng không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    wTrangChu customerWindow = new wTrangChu(currentCustomer.userID);
                    customerWindow.Show();
                    this.Close();
                }
                else if (currentUser is Users currentAdmin)
                {
                    if (currentAdmin.userID <= 0)
                    {
                        System.Windows.MessageBox.Show("Lỗi: ID quản trị viên không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    wTrangChu_Admin adminWindow = new wTrangChu_Admin();
                    adminWindow.Show();
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
            wDangKy registerWindow = new wDangKy();
            registerWindow.Show();
            this.Close();
        }
    }
}