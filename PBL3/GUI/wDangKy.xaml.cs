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

        // Xử lý nút ẩn/hiện mật khẩu
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

        // Xử lý nút quay lại Đăng nhập
        private void lblDangNhap_Click(object sender, MouseButtonEventArgs e)
        {
            wDangNhap loginWindow = new wDangNhap();
            loginWindow.Show();
            this.Close();
        }

        // Xử lý nút Đăng ký
        private void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ giao diện
            string fullName = txtFullName.Text;
            string phoneNumber = txtPhoneNumber.Text;
            string password = isPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;

            // 2. Kiểm tra dữ liệu rỗng
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập đầy đủ thông tin (Họ tên, Số điện thoại, Mật khẩu)!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Gọi hàm xử lý logic bên CustomerManagers
            bool isSuccess = CustomerManagers.Register(fullName, phoneNumber, password);

            // 4. Hiển thị thông báo và điều hướng
            if (isSuccess)
            {
                System.Windows.MessageBox.Show("Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Đăng ký xong tự động mở lại trang Đăng nhập
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