using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Interface;
using PBL3.Manangers;
using PBL3.Models;
using PBL3.Data;

namespace PBL3.GUI
{
    public partial class wTrangChu : Window
    {
        public ObservableCollection<ProductViewModel>? ProductList { get; set; }
        private List<ProductViewModel>? allProducts;

        private string currentCategory = "Tất cả";
        private bool isSortAscending = true;

        private int _currentCustomerId;
        private double _currentDiscount = 0;
        private CustomerPointService _pointService = new CustomerPointService();

        public wTrangChu(int loggedInCustomerId)
        {
            InitializeComponent();
            _currentCustomerId = loggedInCustomerId;

            LoadDataFromDatabase();

            icGioHang.ItemsSource = CartManager.GioHang;

            LoadThongTinKhachHang();
            CapNhatSoLuongGioHang();
        }

        private void LoadThongTinKhachHang()
        {
            try
            {
                using (var db = new MilkTeaDBContext())
                {
                    var customer = db.Customers.FirstOrDefault(c => c.userID == _currentCustomerId);
                    if (customer != null)
                    {
                        string rank = _pointService.GetCustomerRank(customer.userID);
                        _currentDiscount = _pointService.GetDiscountPercentage(customer.userID);

                        lblThongTinKhach.Text = $"Xin chào {customer.Name} | Hạng: {rank} (Giảm {_currentDiscount * 100}%) | Điểm: {customer.point}";
                    }
                    else
                    {
                        lblThongTinKhach.Text = "Khách vãng lai";
                        _currentDiscount = 0;
                    }
                }
                TinhTongTien();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi khi tải thông tin khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                lblThongTinKhach.Text = "Khách vãng lai";
                _currentDiscount = 0;
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                ProductList = new ObservableCollection<ProductViewModel>();
                allProducts = new List<ProductViewModel>();

                ItemManager itemManager = new ItemManager();
                var dbItems = itemManager.GetAllMenuItems();

                if (dbItems != null && dbItems.Count > 0)
                {
                    foreach (var item in dbItems)
                    {
                        string dbPath = string.IsNullOrEmpty(item.ImagePath) ? "/Images/default.jpg" : item.ImagePath;
                        string fullImagePath = $"pack://application:,,,{dbPath}";

                        var product = new ProductViewModel
                        {
                            ItemID = item.itemID,
                            Name = item.itemName,
                            Description = $"Size: {item.size} | Loại: {item.itemType}",
                            Price = $"{item.price:N0}đ",
                            Badge = item.isAvailable ? "SẴN SÀNG" : "TẠM HẾT",
                            ImagePath = fullImagePath,
                            Category = item.itemType
                        };

                        allProducts.Add(product);
                    }
                }

                FilterProducts("Tất cả");
                if (icProducts != null)
                {
                    icProducts.ItemsSource = ProductList;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi khi tải dữ liệu sản phẩm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductList = new ObservableCollection<ProductViewModel>();
                allProducts = new List<ProductViewModel>();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts(currentCategory);
        }

        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.RadioButton rb && rb.IsChecked == true && allProducts != null)
            {
                string categoryName = rb.Content.ToString() ?? "Tất cả";
                FilterProducts(categoryName);
            }
        }

        private void FilterProducts(string category)
        {
            currentCategory = category;

            if (ProductList == null || allProducts == null) return;

            ProductList.Clear();

            IEnumerable<ProductViewModel> filtered;

            if (category == "Tất cả")
            {
                filtered = allProducts;
            }
            else if (category == "Món khác")
            {
                filtered = allProducts.Where(p => p.Category != "Milk Tea" && p.Category != "Fruit Tea");
            }
            else
            {
                filtered = allProducts.Where(p => p.Category == category);
            }

            if (txtSearch != null)
            {
                string keyword = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = filtered.Where(p => p.Name != null && p.Name.ToLower().Contains(keyword));
                }
            }

            if (isSortAscending)
                filtered = filtered.OrderBy(p => ParsePrice(p.Price));
            else
                filtered = filtered.OrderByDescending(p => ParsePrice(p.Price));

            if (!filtered.Any())
            {
                ProductList.Add(new ProductViewModel
                {
                    Name = "Không tìm thấy món",
                    Description = "Không có sản phẩm nào phù hợp",
                    Price = "0đ",
                    Badge = "TRỐNG"
                });
            }
            else
            {
                foreach (var item in filtered)
                {
                    ProductList.Add(item);
                }
            }
        }

        private void TinhTongTien()
        {
            if (lblTongTien != null && lblTamTinh != null)
            {
                int tongTienGoc = 0;
                foreach (var item in CartManager.GioHang) tongTienGoc += item.ThanhTien;

                lblTamTinh.Text = $"{tongTienGoc:N0}đ";

                int tienGiam = (int)(tongTienGoc * _currentDiscount);
                if (lblGiamGia != null) lblGiamGia.Text = $"-{tienGiam:N0}đ";

                int tienThanhToan = tongTienGoc - tienGiam;
                lblTongTien.Text = $"{tienThanhToan:N0}đ";
            }
        }

        private void CapNhatSoLuongGioHang()
        {
            if (lblCartCount == null)
            {
                return;
            }

            int totalCount = 0;
            foreach (var item in CartManager.GioHang) 
            {
                totalCount += item.SoLuong;
            }
            lblCartCount.Text = $"{totalCount} Món";
        }

        private void btnTangSoLuong_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartItem item)
            {
                item.SoLuong++;
                int index = CartManager.GioHang.IndexOf(item);
                CartManager.GioHang.RemoveAt(index);
                CartManager.GioHang.Insert(index, item);

                TinhTongTien();
                CapNhatSoLuongGioHang();
            }
        }

        private void btnGiamSoLuong_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartItem item)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong--;
                    int index = CartManager.GioHang.IndexOf(item);
                    CartManager.GioHang.RemoveAt(index);
                    CartManager.GioHang.Insert(index, item);
                }
                else
                {
                    var result = System.Windows.MessageBox.Show($"Xóa '{item.TenMon}' khỏi giỏ hàng?", "Xác nhận", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        CartManager.GioHang.Remove(item);
                    }
                }
                TinhTongTien();
                CapNhatSoLuongGioHang();
            }
        }

        private void btnSuaMon_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartItem itemToEdit)
            {
                string imagePath = "/Images/default.png";

                string basePrice = $"{itemToEdit.GiaGoc}đ";

                if (allProducts != null)
                {
                    var sp = allProducts.FirstOrDefault(p => p.ItemID == itemToEdit.ItemID);
                    if (sp != null)
                    {
                        imagePath = sp.ImagePath ?? "/Images/default.png";
                        basePrice = sp.Price ?? $"{itemToEdit.GiaGoc}đ";
                    }
                }

                wChiTietMon detailWindow = new wChiTietMon(itemToEdit.ItemID, itemToEdit.TenMon ?? "", basePrice, imagePath);

                detailWindow.LoadEditData(itemToEdit);
                detailWindow.ShowDialog();

                TinhTongTien();
                CapNhatSoLuongGioHang();
            }
        }

        private void btnXoaMon_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartItem itemToXoa)
            {
                var result = System.Windows.MessageBox.Show($"Bạn có chắc muốn xóa '{itemToXoa.TenMon}'?", "Xác nhận", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    CartManager.GioHang.Remove(itemToXoa);
                    TinhTongTien();
                    CapNhatSoLuongGioHang();
                }
            }
        }

        private void btnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (CartManager.GioHang.Count == 0)
            {
                System.Windows.MessageBox.Show("Giỏ hàng đang trống!", "Cảnh báo");
                return;
            }

            var result = System.Windows.MessageBox.Show("Xác nhận tạo đơn hàng và thanh toán?", "Thanh Toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int tongTienGoc = 0;
                List<OrderDetails> listOrderDetails = new List<OrderDetails>();

                foreach (var cartItem in CartManager.GioHang)
                {
                    tongTienGoc += cartItem.ThanhTien;

                    int giaThucBan = (int)(cartItem.GiaGoc * (1 - _currentDiscount));

                    listOrderDetails.Add(new OrderDetails
                    {
                        itemID = cartItem.ItemID,
                        size = cartItem.Size ?? "M",
                        quantity = cartItem.SoLuong,
                        priceAtOrder = giaThucBan,
                        note = cartItem.MoTa
                    });
                }

                int tienGiam = (int)(tongTienGoc * _currentDiscount);
                int finalPrice = tongTienGoc - tienGiam;

                Orders newOrder = new Orders()
                {
                    customerID = _currentCustomerId,
                    staffID = null,
                    orderDate = System.DateTime.Now,
                    orderStatus = "Pending",
                    totalPrice = finalPrice
                };

                OrderService orderService = new OrderService();
                bool isSuccess = orderService.CreateOrder(newOrder, listOrderDetails);

                if (isSuccess)
                {
                    int oldPoints = _pointService.GetCurrentPoints(_currentCustomerId);
                    _pointService.AddPoints(_currentCustomerId, finalPrice);
                    int newPoints = _pointService.GetCurrentPoints(_currentCustomerId);

                    string msg = $"Đặt hàng thành công! Tổng thanh toán: {finalPrice:N0}đ\nĐơn hàng đang chờ xử lý.";

                    if (oldPoints < 100 && newPoints >= 100) msg += "\n\n🎉 CHÚC MỪNG! Bạn đã thăng hạng ĐỒNG!";
                    else if (oldPoints < 200 && newPoints >= 200) msg += "\n\n🎉 CHÚC MỪNG! Bạn đã thăng hạng BẠC!";
                    else if (oldPoints < 300 && newPoints >= 300) msg += "\n\n🎉 CHÚC MỪNG! Bạn đã thăng hạng VÀNG!";

                    System.Windows.MessageBox.Show(msg, "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);

                    int diemDuocCongThem = newPoints - oldPoints;

                    wHoaDon hoaDonWindow = new wHoaDon(newOrder.orderID, lblTongTien.Text, diemDuocCongThem, newPoints, null);
                    hoaDonWindow.ShowDialog();

                    CartManager.GioHang.Clear();
                    LoadThongTinKhachHang();
                    CapNhatSoLuongGioHang();
                }
                else
                {
                    System.Windows.MessageBox.Show("Lỗi kết nối máy chủ. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProductCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.Border clickedBorder = sender as System.Windows.Controls.Border;
            if (clickedBorder != null && clickedBorder.DataContext is ProductViewModel selectedProduct)
            {
                if (selectedProduct.Badge == "TRỐNG") return;

                wChiTietMon detailWindow = new wChiTietMon(selectedProduct.ItemID, selectedProduct.Name ?? "", selectedProduct.Price ?? "", selectedProduct.ImagePath ?? "");
                detailWindow.ShowDialog();

                CapNhatSoLuongGioHang();
                TinhTongTien();
            }
        }

        private void btnSort_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProductList == null || ProductList.Count == 0) return;

            isSortAscending = !isSortAscending;

            if (isSortAscending)
                txtSortMode.Text = "Giá: Thấp đến cao   \xE70D";
            else
                txtSortMode.Text = "Giá: Cao đến thấp   \xE70E";

            FilterProducts(currentCategory);
        }

        private int ParsePrice(string? priceString)
        {
            if (string.IsNullOrEmpty(priceString)) return 0;
            string cleanString = priceString.Replace(".", "").Replace(",", "").Replace("đ", "").Trim();
            if (int.TryParse(cleanString, out int result)) return result;
            return 0;
        }

        private void lblDangXuat_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Bạn có chắc chắn muốn đăng xuất khỏi tài khoản?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                wDangNhap loginWindow = new wDangNhap();
                loginWindow.Show();

                this.Close();
            }
        }
        private void lblLichSu_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentCustomerId == 1)
            {
                System.Windows.MessageBox.Show("Khách vãng lai không có lịch sử đơn hàng. Vui lòng đăng nhập tài khoản thành viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            wLichSuDonHang lichSuWindow = new wLichSuDonHang(_currentCustomerId);
            lichSuWindow.ShowDialog();
        }
    }

    public class ProductViewModel
    {
        public int ItemID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public string? Badge { get; set; }
        public string? ImagePath { get; set; }
        public string? Category { get; set; }
    }
}