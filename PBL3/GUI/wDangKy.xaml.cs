using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PBL3.Manangers;

namespace PBL3.GUI
{
    public partial class wDangKy : Window
    {
        private bool isPasswordVisible = false;

        public wDangKy()
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

        private void lblDangNhap_Click(object sender, MouseButtonEventArgs e)
        {
            wDangNhap loginWindow = new wDangNhap();
            loginWindow.Show();
            this.Close();
        }

        private void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text?.Trim() ?? "";
            string phoneNumber = txtPhoneNumber.Text?.Trim() ?? "";
            string password = isPasswordVisible ? txtPasswordVisible.Text?.Trim() ?? "" : txtPassword.Password?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(fullName))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập họ tên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập số điện thoại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (phoneNumber.Length < 10)
            {
                System.Windows.MessageBox.Show("Số điện thoại phải có ít nhất 10 ký tự!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                System.Windows.MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSuccess = CustomerManagers.Register(fullName, phoneNumber, password);

            if (isSuccess)
            {
                System.Windows.MessageBox.Show("Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                wDangNhap loginWindow = new wDangNhap();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Đăng ký thất bại: Số điện thoại này có thể đã được đăng ký từ trước. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}