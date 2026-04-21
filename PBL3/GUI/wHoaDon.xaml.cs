using System;
using System.Windows;

namespace PBL3.GUI
{
    public partial class wHoaDon : Window
    {
        public wHoaDon(int orderId, string tongTien)
        {
            InitializeComponent();

            lblMaDon.Text = $"#{orderId:D4}";

            lblNgayGio.Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm");

            icHoaDon.ItemsSource = CartManager.GioHang;

            lblTamTinh.Text = tongTien;
            lblTongTienHoaDon.Text = tongTien;
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}