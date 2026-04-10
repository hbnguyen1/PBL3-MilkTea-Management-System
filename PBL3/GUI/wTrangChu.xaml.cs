using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Interface;
using PBL3.Manangers;
using PBL3.Models;

namespace PBL3.GUI
{
    public partial class wTrangChu : Window
    {
        public ObservableCollection<ProductViewModel>? ProductList { get; set; }
        private List<ProductViewModel>? allProducts;

        // Trạng thái bộ lọc và sắp xếp hiện tại
        private string currentCategory = "Tất cả";
        private bool isSortAscending = true;

        public wTrangChu()
        {
            InitializeComponent();
            LoadDataFromDatabase();

            icGioHang.ItemsSource = CartManager.GioHang;

            CapNhatSoLuongGioHang();
            TinhTongTien();
        }

        private void LoadDataFromDatabase()
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

            // Mặc định gọi hàm lọc để hiển thị "Tất cả" lên màn hình
            FilterProducts("Tất cả");

            icProducts.ItemsSource = ProductList;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mỗi khi khách gõ phím, gọi lại hàm lọc
            FilterProducts(currentCategory);
        }

        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem nút nào vừa được bấm
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

            // 1. Lọc theo danh mục
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
                string keyword = txtSearch.Text.Trim().ToLower(); // Lấy chữ người dùng gõ
                if (!string.IsNullOrEmpty(keyword))
                {
                    // Lọc ra các món có tên chứa từ khóa (không phân biệt hoa/thường)
                    filtered = filtered.Where(p => p.Name != null && p.Name.ToLower().Contains(keyword));
                }
            }

            // 3. Kết hợp Sắp xếp hiện tại vào kết quả vừa lọc
            if (isSortAscending)
                filtered = filtered.OrderBy(p => ParsePrice(p.Price));
            else
                filtered = filtered.OrderByDescending(p => ParsePrice(p.Price));

            // 4. Đổ dữ liệu ra màn hình
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
            if (lblTongTien != null)
            {
                int tongTien = 0;
                foreach (var item in CartManager.GioHang) tongTien += item.ThanhTien;
                lblTongTien.Text = $"{tongTien:N0}đ";
            }
        }

        private void CapNhatSoLuongGioHang()
        {
            if (lblCartCount != null)
            {
                int totalCount = 0;
                foreach (var item in CartManager.GioHang) totalCount += item.SoLuong;
                lblCartCount.Text = $"{totalCount} Món";
            }
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

            var result = System.Windows.MessageBox.Show("Xác nhận tạo đơn hàng và in hóa đơn?", "Thanh Toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // 1. Chuyển đổi giỏ hàng sang Model
                int tongTien = 0;
                List<OrderDetails> listOrderDetails = new List<OrderDetails>();

                foreach (var cartItem in CartManager.GioHang)
                {
                    tongTien += cartItem.ThanhTien;
                    listOrderDetails.Add(new OrderDetails
                    {
                        itemID = cartItem.ItemID,
                        size = cartItem.Size ?? "M",
                        quantity = cartItem.SoLuong,
                        priceAtOrder = cartItem.GiaGoc,
                        note = cartItem.MoTa
                    });
                }

                // 2. Tạo đối tượng Order (Sửa lại customerID và staffID thực tế của bạn nếu cần)
                Orders newOrder = new Orders()
                {
                    customerID = 1,
                    staffID = 101,
                    orderDate = System.DateTime.Now,
                    orderStatus = "Pending",
                    totalPrice = tongTien
                };

                // 3. Lưu vào Database và LẤY ID THỰC TẾ
                OrderService orderService = new OrderService();
                int savedOrderId = orderService.CreateOrder(newOrder, listOrderDetails);

                if (savedOrderId > 0) // Lưu thành công
                {
                    // 4. Mở tờ hóa đơn và truyền mã Đơn hàng (savedOrderId) thật vào
                    wHoaDon hoaDonWindow = new wHoaDon(savedOrderId, lblTongTien.Text);
                    hoaDonWindow.ShowDialog();

                    // Làm sạch giao diện đón khách mới
                    CartManager.GioHang.Clear();
                    TinhTongTien();
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

                // Cập nhật sau khi Popup đóng
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
                // Mở lại trang đăng nhập
                wDangNhap loginWindow = new wDangNhap();
                loginWindow.Show();

                // Đóng trang chủ hiện tại
                this.Close();
            }
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