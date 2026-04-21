using System;
using System.Windows;

namespace PBL3.GUI
{
    public partial class wHoaDon : Window
    {
        // ĐÃ SỬA: Thêm 2 tham số diemCong và tongDiem
        public wHoaDon(int orderId, string tongTien, int diemCong, int tongDiem)
        {
            InitializeComponent();

            lblMaDon.Text = $"#{orderId:D4}";
            lblNgayGio.Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm");
            icHoaDon.ItemsSource = CartManager.GioHang;
            lblTamTinh.Text = tongTien;
            lblTongTienHoaDon.Text = tongTien;

            // Nạp dữ liệu điểm lên giao diện
            lblDiemCong.Text = $"+{diemCong}";
            lblTongDiem.Text = $"{tongDiem} điểm";

            // Nếu không có điểm (ví dụ khách vãng lai) thì ẩn khung điểm đi
            if (diemCong == 0 && tongDiem == 0)
            {
                bdDiemTichLuy.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}