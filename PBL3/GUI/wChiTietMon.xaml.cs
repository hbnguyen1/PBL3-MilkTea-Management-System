using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;

namespace PBL3.GUI
{
    public partial class wChiTietMon : Window
    {
        // 1. Khai báo biến này để giữ lại Mã món ăn
        private int _currentItemId;

        // 2. Cập nhật hàm khởi tạo để nhận int itemId từ Trang Chủ
        public wChiTietMon(int itemId, string tenMon, string gia, string imagePath)
        {
            InitializeComponent();

            _currentItemId = itemId; // Lưu mã món ăn lại để lát nữa thêm vào giỏ

            // Gán dữ liệu nhận được từ Trang Chủ lên Form Chi Tiết
            lblTenMon.Text = tenMon;
            lblGia.Text = gia;

            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    imgBrush.ImageSource = bitmap;
                }
                catch (Exception)
                {
                    imgMonAnBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
                }
            }
        }

        private void btnThemVaoGio_Click(object sender, RoutedEventArgs e)
        {
            string size = radSizeL.IsChecked == true ? "L" : "M";

            string duong = radDuong100.IsChecked == true ? "100% Đường" : (radDuong70.IsChecked == true ? "70% Đường" : "50% Đường");
            string da = radDa100.IsChecked == true ? "100% Đá" : "50% Đá";
            string moTa = $"Size {size}, {duong}, {da}";

            string ghiChu = txtGhiChu.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(ghiChu))
            {
                moTa += $"\nGhi chú: {ghiChu}";
            }

            string giaGocChuoi = lblGia.Text.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            int giaGoc = int.TryParse(giaGocChuoi, out int price) ? price : 0;

            if (size == "L") giaGoc += 8000;

            var existingItem = CartManager.GioHang.FirstOrDefault(x => x.ItemID == _currentItemId && x.MoTa == moTa);

            if (existingItem != null)
            {
                // Đã có -> Tăng số lượng
                existingItem.SoLuong += 1;

                int index = CartManager.GioHang.IndexOf(existingItem);
                CartManager.GioHang.RemoveAt(index);
                CartManager.GioHang.Insert(index, existingItem);
            }
            else
            {
                // Chưa có -> Thêm mới
                CartItem newItem = new CartItem()
                {
                    ItemID = _currentItemId,
                    Size = size,
                    TenMon = lblTenMon.Text.Trim(), // Trim để cắt sạch khoảng trắng thừa
                    MoTa = moTa,
                    GiaGoc = giaGoc,
                    SoLuong = 1
                };
                CartManager.GioHang.Add(newItem);
            }

            System.Windows.MessageBox.Show("Đã thêm món vào giỏ hàng!", "Thông báo");
            this.Close();
        }
    }
}