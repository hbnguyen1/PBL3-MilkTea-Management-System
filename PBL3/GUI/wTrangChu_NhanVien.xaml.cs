using System.Windows;
using System.Windows.Input;

namespace PBL3.GUI
{
    public partial class wTrangChu_NhanVien : Window
    {
        public wTrangChu_NhanVien()
        {
            InitializeComponent();
        }

        private void lblDangXuat_Click(object sender, MouseButtonEventArgs e)
        {
            wDangNhap loginWindow = new wDangNhap();
            loginWindow.Show();
            this.Close();
        }

        private void btnQuanLyDonHang_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent != null)
            {
                MainContent.Content = new ucQuanLyDonHang();
            }
        }
    }
}