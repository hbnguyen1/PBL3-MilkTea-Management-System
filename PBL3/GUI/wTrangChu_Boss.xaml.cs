using System;
using System.Windows;
using System.Windows.Input;

namespace PBL3.GUI
{
    public partial class wTrangChu_Boss : Window
    {
        public wTrangChu_Boss()
        {
            InitializeComponent();
            MainContent.Content = new ucBossTongQuan();
        }

        private void lblDangXuat_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Boss có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                wDangNhap loginWindow = new wDangNhap();
                loginWindow.Show();
                this.Close();
            }
        }

        private void btnTongQuan_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;
            MainContent.Content = new ucBossTongQuan();
        }

        private void btnNhanSu_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContent == null) return;
            MainContent.Content = new ucQuanLyNhanVien();
        }
    }
}