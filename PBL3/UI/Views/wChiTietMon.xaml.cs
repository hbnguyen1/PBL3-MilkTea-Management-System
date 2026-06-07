using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using PBL3.src.Application.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace PBL3.UI.Views
{
    public partial class wChiTietMon : Window
    {
        private int _currentItemId;
        private int _giaGocBanDau;
        private CartItem? _editingItem = null;
        private readonly IItemService _itemService;
        private string _loaiMon;
        private string _currentImagePath;
        public wChiTietMon(int itemId, string tenMon, string type, string gia, string imagePath)
        {
            InitializeComponent();
            _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();

            _currentItemId = itemId;
            _loaiMon = type;
            _currentImagePath = imagePath;

            lblTenMon.Text = tenMon;

            string giaClean = gia.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            int.TryParse(giaClean, out _giaGocBanDau);

            lblGia.Text = $"{_giaGocBanDau:N0}đ";
            bool isSizeLAvailable = _itemService.HasEnoughIngredients(_currentItemId, "L", CartManager.GioHang);
            if (!isSizeLAvailable)
            {
                //Nếu size L không đủ thì sẽ bị làm mờ đi
                if (radSizeL != null)
                {
                    radSizeL.IsEnabled = false;
                    radSizeL.Opacity = 0.4;
                    radSizeL.Content = "Size L (Hết nguyên liệu)";
                }

                //Ép hệ thống nhận size M
                if (radSizeM != null)
                {
                    radSizeM.IsChecked = true;
                }
            }

            if (type == "Topping")
            {
                radSizeL.Visibility = Visibility.Collapsed;
                radSizeM.Visibility = Visibility.Collapsed;
                txtDuong.Visibility = Visibility.Collapsed;
                sldDuong.Visibility = Visibility.Collapsed;
                sldDa.Visibility = Visibility.Collapsed;
                txtGhiChu.Visibility = Visibility.Collapsed;
                tbGhichu.Visibility = Visibility.Collapsed;
                txtDa.Visibility = Visibility.Collapsed;
                bdGhichu.Visibility = Visibility.Collapsed;
                txtDavl.Visibility = Visibility.Collapsed;
                txtDuongvl.Visibility = Visibility.Collapsed;
                txtSizeSelect.Visibility = Visibility.Collapsed;
            }

            LoadImage(imagePath);
        }

        private void LoadImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                imgMonAn.Source = bitmap;
            }
            catch (Exception)
            {
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
                        sldDuong.Value = 100;
                    }

                    var matchDa = Regex.Match(item.MoTa, @"(\d+)%\s*Đá");
                    if (matchDa.Success && double.TryParse(matchDa.Groups[1].Value, out double daValue))
                    {
                        sldDa.Value = daValue;
                    }
                    else
                    {
                        sldDa.Value = 50;
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
            //Phân loại logic mô tả dựa trên loại món
            string loai = _loaiMon;
            string moTa = "";
            string size = "";

            if (loai == "Topping")
            {
                moTa = "Topping thêm";
                size = "M"; //Topping không dùng size
            }
            else
            {
                //Nếu là đồ uống, lấy thông tin từ giao diện
                size = radSizeL.IsChecked == true ? "L" : "M";
                string duong = $"{(int)sldDuong.Value}% Đường";
                string da = $"{(int)sldDa.Value}% Đá";
                moTa = $"Size {size}, {duong}, {da}";
            }

            // Gộp ghi chú nếu có
            string ghiChu = txtGhiChu.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(ghiChu))
            {
                moTa += $"\nGhi chú: {ghiChu}";
            }

            //Lấy giá tiền hiện tại từ lblGia
            string giaGocChuoi = lblGia.Text ?? "0";
            giaGocChuoi = giaGocChuoi.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            int giaGoc = int.TryParse(giaGocChuoi, out int price) ? price : 0;

            // Xử lý trường hợp cập nhật món đã có trong giỏ (nếu đang ở chế độ chỉnh sửa)
            if (_editingItem != null)
            {
                var existingSameItem = CartManager.GioHang.FirstOrDefault(x => x != _editingItem && x.ItemID == _currentItemId && x.MoTa == moTa);

                if (existingSameItem != null)
                {
                    existingSameItem.SoLuong += _editingItem.SoLuong;
                    CartManager.GioHang.Remove(_editingItem);
                    // Refresh lại vị trí trong ObservableCollection để giao diện cập nhật
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

            //Xử lý trường hợp thêm món mới vào giỏ
            var existingItem = CartManager.GioHang.FirstOrDefault(x => x.ItemID == _currentItemId && x.MoTa == moTa);

            if (existingItem != null)
            {
                existingItem.SoLuong += 1;
                //Cập nhật lại vị trí để kích hoạt UI Binding
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
                    Loai = loai,
                    MoTa = moTa,
                    GiaGoc = giaGoc,
                    ImagePath = _currentImagePath,
                    SoLuong = 1
                };
                CartManager.GioHang.Add(newItem);
            }

            System.Windows.MessageBox.Show("Đã thêm món vào giỏ hàng!", "Thông báo");
            this.Close();
        }
        private void sldDuong_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}