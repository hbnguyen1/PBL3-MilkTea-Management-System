using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace PBL3.GUI
{
    public partial class wTrangChu_NhanVien : Window
    {
        // 1. Khai báo sẵn các biến chứa giao diện
        private ucTrangChuNhanVien? _ucTrangChu;
        private ucQuanLyDonHang? _ucQuanLyDonHang;
        private ucQuanLyKho? _ucQuanLyKho;
        private ucQuanLyMon? _ucQuanLyMon;

        public wTrangChu_NhanVien()
        {
            InitializeComponent();

            // Khởi tạo tab mặc định khi vừa mở app
            _ucTrangChu = new ucTrangChuNhanVien();
            MainContent.Content = _ucTrangChu;
        }

        private void lblDangXuat_Click(object sender, MouseButtonEventArgs e)
        {
            wDangNhap loginWindow = Program.ServiceProvider.GetRequiredService<wDangNhap>();
            loginWindow.Show();
            this.Close();
        }

        private void btnTongQuan_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            // 2. Kiểm tra xem đã tạo chưa, chưa thì mới new
            if (_ucTrangChu == null)
            {
                _ucTrangChu = new ucTrangChuNhanVien();
            }
            MainContent.Content = _ucTrangChu;
        }

        private void btnQuanLyDonHang_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            if (_ucQuanLyDonHang == null)
            {
                _ucQuanLyDonHang = new ucQuanLyDonHang();
            }
            MainContent.Content = _ucQuanLyDonHang;
        }

        private void btnMenuKho_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            if (_ucQuanLyKho == null)
            {
                _ucQuanLyKho = new ucQuanLyKho();
            }
            MainContent.Content = _ucQuanLyKho;
        }

        private void btnQuanLyMon_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            if (_ucQuanLyMon == null)
            {
                _ucQuanLyMon = new ucQuanLyMon();
            }
            MainContent.Content = _ucQuanLyMon;
        }
    }
}