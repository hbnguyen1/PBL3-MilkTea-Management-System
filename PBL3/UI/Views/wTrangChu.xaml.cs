using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Application.Interface;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace PBL3.UI.Views
{
    public partial class wTrangChu : Window
    {
        public ObservableCollection<ProductViewModel>? ProductList { get; set; }
        private List<ProductViewModel>? allProducts;

        private string currentCategory = "Tất cả";
        private bool isSortAscending = false;

        private int _currentCustomerId;
        private double _currentDiscount = 0;

        private readonly ICustomerPointService _pointService;
        private readonly IItemService _itemService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public wTrangChu(int loggedInCustomerId)
        {
            InitializeComponent();
            _currentCustomerId = loggedInCustomerId;

            _pointService = Program.ServiceProvider.GetRequiredService<ICustomerPointService>();
            _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();
            _orderService = Program.ServiceProvider.GetRequiredService<IOrderService>();
            _userService = Program.ServiceProvider.GetRequiredService<IUserService>();

            LoadDataFromDatabase();

            icGioHang.ItemsSource = CartManager.GioHang;

            LoadThongTinKhachHang();
            CapNhatSoLuongGioHang();
        }

        private void LoadThongTinKhachHang()
        {
            try
            {
                var customer = _userService.GetUserById(_currentCustomerId);
                if (customer != null)
                {
                    string rank = _pointService.GetCustomerRank(customer.userID);
                    _currentDiscount = _pointService.GetDiscountPercentage(customer.userID);
                    int points = 0;
                    try { points = ((dynamic)customer).point; } catch { }

                    lblThongTinKhach.Text = $"Xin chào {customer.Name} | Hạng: {rank} (Giảm {_currentDiscount * 100}%) | Điểm: {points}";
                }
                else
                {
                    lblThongTinKhach.Text = "Khách vãng lai";
                    _currentDiscount = 0;
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

                var dbItems = _itemService.GetAllItems()
                                          .Where(i => i.isAvailable && i.size == "M") 
                                          .OrderBy(item => item.itemID)
                                          .ToList();

                if (dbItems != null && dbItems.Count > 0)
                {
                    foreach (var item in dbItems)
                    {
                        string dbPath = string.IsNullOrEmpty(item.FullImagePath) ? "/Images/default.jpg" : item.FullImagePath;
                        string fullImagePath = $"pack://application:,,,{dbPath}";

                        // Check if recipes exist for available sizes
                        bool hasRecipeM = item.Recipes != null && item.Recipes.Any(r => r.size == "M");
                        bool hasRecipeL = item.Recipes != null && item.Recipes.Any(r => r.size == "L");
                        //bool isAvailable = hasRecipeM || hasRecipeL;
                        bool isAvailable = _itemService.isAvailable(item);

                        var product = new ProductViewModel
                        {
                            ItemID = item.itemID,
                            Name = item.itemName,
                            Description = $"Size: {item.size} | Loại: {item.itemType}",
                            Price = $"{item.price:N0}đ",
                            Badge = isAvailable ? "SẴN SÀNG" : "TẠM HẾT",
                            ImagePath = item.ImagePath,
                            FullImagePath = item.FullImagePath,
                            Category = item.itemType,
                            isAvailable = item.isAvailable
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
            if (lblCartCount == null) return;

            int totalCount = 0;
            foreach (var item in CartManager.GioHang)
            {
                totalCount += item.SoLuong;
            }
            lblCartCount.Text = $"{totalCount} Món";

            // ==========================================================
            // ÁP DỤNG "KHO ẢO" LÊN GIAO DIỆN (Ý TƯỞNG CỦA BẠN NẰM Ở ĐÂY)
            // ==========================================================
            if (allProducts != null && _itemService != null && icProducts != null)
            {
                // Lấy danh sách ID các món đã bị hết hàng (dựa theo kho ảo trên RAM)
                var outOfStockIds = _itemService.GetOutOfStockItemsVirtually(CartManager.GioHang);

                // Quét qua danh sách hiển thị
                foreach (var p in allProducts)
                {
                    if (outOfStockIds.Contains(p.ItemID))
                    {
                        p.Badge = "TẠM HẾT"; // Gán nhãn tạm hết
                    }
                    else
                    {
                        p.Badge = "SẴN SÀNG"; // Trả lại trạng thái sẵn sàng
                    }
                }

                // Lệnh thần thánh: Ép toàn bộ thẻ món ăn vẽ lại (Áp dụng luôn Style làm mờ bên XAML)
                icProducts.Items.Refresh();
            }
        }

        private void btnTangSoLuong_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartItem item)
            {
                var outOfStockIds = _itemService.GetOutOfStockItemsVirtually(CartManager.GioHang);

                if (outOfStockIds.Contains(item.ItemID))
                {
                    System.Windows.MessageBox.Show($"Kho đã cạn nguyên liệu, không thể tăng thêm số lượng cho món '{item.TenMon}'!", "Hết nguyên liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Khóa mõm, dừng hàm ngay lập tức
                }
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
                string imagePath = itemToEdit.ImagePath ?? "/Images/default.png";

                string basePrice = $"{itemToEdit.GiaGoc}đ";

                wChiTietMon detailWindow = new wChiTietMon(itemToEdit.ItemID, itemToEdit.TenMon ?? "", itemToEdit.Loai  ,basePrice, imagePath);

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

                bool isSuccess = _orderService.CreateOrder(newOrder, listOrderDetails);

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

                    wHoaDon hoaDonWindow = new wHoaDon(newOrder.orderID, lblTongTien.Text, diemDuocCongThem, newPoints, CartManager.GioHang.ToList());
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
                //Kiểm tra Badge 
                if (selectedProduct.Badge == "TẠM HẾT")
                {
                    System.Windows.MessageBox.Show("Rất tiếc, món này hiện đã hết nguyên liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //Kiểm tra nếu là TRỐNG (dùng cho khi không tìm thấy kết quả)
                if (selectedProduct.Badge == "TRỐNG") return;

                // Nếu qua được các bước trên thì mới cho mở cửa sổ chi tiết
                wChiTietMon detailWindow = new wChiTietMon(
                    selectedProduct.ItemID,
                    selectedProduct.Name ?? "",
                    selectedProduct.Category ?? "",
                    selectedProduct.Price ?? "",
                    selectedProduct.FullImagePath ?? ""
                );
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
                wDangNhap loginWindow = Program.ServiceProvider.GetRequiredService<wDangNhap>();
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
        public string? FullImagePath { get; set; }
        public string? Category { get; set; }
        public bool isAvailable { get; set; }
    }
}