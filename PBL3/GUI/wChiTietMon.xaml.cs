using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PBL3.GUI
{
    public partial class wChiTietMon : Window
    {
        private int _currentItemId;
        private int _giaGocBanDau;
        private CartItem? _editingItem = null;

        public wChiTietMon(int itemId, string tenMon, string gia, string imagePath)
        {
            InitializeComponent();

            _currentItemId = itemId;
            lblTenMon.Text = tenMon;

            string giaClean = gia.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            int.TryParse(giaClean, out _giaGocBanDau);

            lblGia.Text = $"{_giaGocBanDau:N0}đ";

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

        private void radSizeL_Checked(object sender, RoutedEventArgs e)
        {
            if (lblGia != null && _giaGocBanDau > 0)
            {
                lblGia.Text = $"{(_giaGocBanDau + 8000):N0}đ";
            }
        }

        private void radSizeM_Checked(object sender, RoutedEventArgs e)
        {
            if (lblGia != null && _giaGocBanDau > 0)
            {
                lblGia.Text = $"{_giaGocBanDau:N0}đ";
            }
        }

        public void LoadEditData(CartItem item)
        {
            _editingItem = item;

            if (item.Size == "L") radSizeL.IsChecked = true;
            else radSizeM.IsChecked = true;

            if (item.MoTa != null)
            {
                try
                {
                    var matchDuong = Regex.Match(item.MoTa, @"(\d+)%\s*Đường");
                    if (matchDuong.Success && double.TryParse(matchDuong.Groups[1].Value, out double duongValue))
                    {
                        sldDuong.Value = duongValue;
                    }
                    else
                    {
                        sldDuong.Value = 100;  // Default value
                    }

                    var matchDa = Regex.Match(item.MoTa, @"(\d+)%\s*Đá");
                    if (matchDa.Success && double.TryParse(matchDa.Groups[1].Value, out double daValue))
                    {
                        sldDa.Value = daValue;
                    }
                    else
                    {
                        sldDa.Value = 50;  // Default value
                    }

                    if (item.MoTa.Contains("Ghi chú: "))
                    {
                        int noteIndex = item.MoTa.IndexOf("Ghi chú: ") + 9;
                        if (noteIndex < item.MoTa.Length)
                        {
                            txtGhiChu.Text = item.MoTa.Substring(noteIndex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi khi phân tích dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnThemVaoGio_Click(object sender, RoutedEventArgs e)
        {
            string size = radSizeL.IsChecked == true ? "L" : "M";

            string duong = $"{(int)sldDuong.Value}% Đường";
            string da = $"{(int)sldDa.Value}% Đá";
            string moTa = $"Size {size}, {duong}, {da}";

            string ghiChu = txtGhiChu.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(ghiChu))
            {
                moTa += $"\nGhi chú: {ghiChu}";
            }

            string giaGocChuoi = lblGia.Text.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            int giaGoc = int.TryParse(giaGocChuoi, out int price) ? price : 0;

            if (_editingItem != null)
            {
                var existingSameItem = CartManager.GioHang.FirstOrDefault(x => x != _editingItem && x.ItemID == _currentItemId && x.MoTa == moTa);

                if (existingSameItem != null)
                {
                    existingSameItem.SoLuong += _editingItem.SoLuong;
                    CartManager.GioHang.Remove(_editingItem);

                    int idx = CartManager.GioHang.IndexOf(existingSameItem);
                    CartManager.GioHang.RemoveAt(idx);
                    CartManager.GioHang.Insert(idx, existingSameItem);
                }
                else
                {
                    _editingItem.Size = size;
                    _editingItem.MoTa = moTa;
                    _editingItem.GiaGoc = giaGoc;

                    int index = CartManager.GioHang.IndexOf(_editingItem);
                    CartManager.GioHang.RemoveAt(index);
                    CartManager.GioHang.Insert(index, _editingItem);
                }

                System.Windows.MessageBox.Show("Cập nhật món thành công!", "Thông báo");
                this.Close();
                return;
            }

            var existingItem = CartManager.GioHang.FirstOrDefault(x => x.ItemID == _currentItemId && x.MoTa == moTa);

            if (existingItem != null)
            {
                existingItem.SoLuong += 1;

                int index = CartManager.GioHang.IndexOf(existingItem);
                CartManager.GioHang.RemoveAt(index);
                CartManager.GioHang.Insert(index, existingItem);
            }
            else
            {
                CartItem newItem = new CartItem()
                {
                    ItemID = _currentItemId,
                    Size = size,
                    TenMon = lblTenMon.Text.Trim(),
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