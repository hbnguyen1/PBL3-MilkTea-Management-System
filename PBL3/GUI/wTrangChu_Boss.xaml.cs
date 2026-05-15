using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace PBL3.GUI
{
    public partial class wTrangChu_Boss : Window
    {
        // 1. Khai báo sẵn các biến chứa UserControl để dùng lại
        private ucBossTongQuan? _ucTongQuan;
        private ucQuanLyNhanVien? _ucNhanSu;

        public wTrangChu_Boss()
        {
            InitializeComponent();

            // Khởi tạo lần đầu
            _ucTongQuan = new ucBossTongQuan();
            MainContent.Content = _ucTongQuan;
        }

        private void lblDangXuat_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Boss có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                wDangNhap loginWindow = Program.ServiceProvider.GetRequiredService<wDangNhap>();
                loginWindow.Show();
                this.Close();
            }
        }

        private void btnTongQuan_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            // 2. Nếu chưa tạo thì mới new, tạo rồi thì lôi biến ra gán vào luôn
            if (_ucTongQuan == null)
            {
                _ucTongQuan = new ucBossTongQuan();
            }
            MainContent.Content = _ucTongQuan;
        }

        private void btnNhanSu_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;

            // 2. Tương tự với nhân sự
            if (_ucNhanSu == null)
            {
                _ucNhanSu = new ucQuanLyNhanVien();
            }
            MainContent.Content = _ucNhanSu;
        }
    }
}