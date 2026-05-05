using System.Windows;
using System.Windows.Input;

namespace PBL3.GUI
{
    public partial class wTrangChu_NhanVien : Window
    {
        public wTrangChu_NhanVien()
        {
            InitializeComponent();
            MainContent.Content = new ucTrangChuNhanVien();
        }

        private void lblDangXuat_Click(object sender, MouseButtonEventArgs e)
        {
            wDangNhap loginWindow = new wDangNhap();
            loginWindow.Show();
            this.Close();
        }

        private void btnTongQuan_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;
            MainContent.Content = new ucTrangChuNhanVien();
        }

        private void btnQuanLyDonHang_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ucQuanLyDonHang();
        }

        private void btnMenuKho_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ucQuanLyKho();
        }

        private void btnQuanLyMon_Checked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ucQuanLyMon();
        }
    }
}