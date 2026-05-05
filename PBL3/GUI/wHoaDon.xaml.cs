using System;
using System.Windows;
using System.Collections;

namespace PBL3.GUI
{
    public partial class wHoaDon : Window
    {
        public wHoaDon(int orderId, string tongTien, int diemCong, int tongDiem, IEnumerable danhSachMon)
        {
            InitializeComponent();

            lblMaDon.Text = $"#{orderId:D4}";
            lblNgayGio.Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm");
            lblTamTinh.Text = tongTien;
            lblTongTienHoaDon.Text = tongTien;

            lblDiemCong.Text = $"+{diemCong}";
            lblTongDiem.Text = $"{tongDiem} điểm";

            icHoaDon.ItemsSource = danhSachMon;

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