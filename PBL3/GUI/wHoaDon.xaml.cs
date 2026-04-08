using System;
using System.Windows;

namespace PBL3.GUI
{
    public partial class wHoaDon : Window
    {
        // Đã thêm int orderId vào hàm khởi tạo
        public wHoaDon(int orderId, string tongTien)
        {
            InitializeComponent();

            // 1. Gắn ID thật từ SQL Server vào UI (Thêm số 0 đằng trước cho đẹp, vd: #TTN0015)
            lblMaDon.Text = $"#{orderId:D4}";

            // 2. Hiển thị ngày giờ
            lblNgayGio.Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm");

            // 3. Đổ dữ liệu Giỏ Hàng
            icHoaDon.ItemsSource = CartManager.GioHang;

            // 4. Gán tổng tiền
            lblTamTinh.Text = tongTien;
            lblTongTienHoaDon.Text = tongTien;
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}