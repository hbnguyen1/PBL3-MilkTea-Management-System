using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Interface;
using PBL3.Manangers;
using PBL3.Models;
using PBL3.Data;
using PBL3.Core;

namespace PBL3.GUI
{
    public class POSCartItem
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
        public string size { get; set; }
        public int quantity { get; set; }
        public double price { get; set; }
        public string note { get; set; }
        public double total { get { return quantity * price; } }

        public int SoLuong => quantity;
        public string TenMon => itemName;
        public string MoTa => $"Size {size}" + (string.IsNullOrEmpty(note) ? "" : $" | {note}");
        public string ThanhTienStr => $"{total:N0} đ";
    }

    public partial class ucPOS : System.Windows.Controls.UserControl
    {
        private List<POSCartItem> _gioHang = new List<POSCartItem>();
        private Users _khachHangHienTai = null;

        private string _currentCategory = "Tất cả";

        public ucPOS()
        {
            InitializeComponent();
            LoadMenuMonAn();
        }

        private int GetCurrentStaffId()
        {
            if (UserSession.CurrentUser?.userID > 0)
            {
                return UserSession.CurrentUser.userID;
            }
            return 0;
        }

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMenuMonAn();
        }

        private void btnLocMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn == null) return;

            _currentCategory = btn.Content.ToString();

            StackPanel parentPanel = btn.Parent as StackPanel;
            if (parentPanel != null)
            {
                foreach (var child in parentPanel.Children)
                {
                    if (child is System.Windows.Controls.Button b)
                    {
                        b.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 244, 246));
                        b.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 74, 74));
                    }
                }
            }

            btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26));
            btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);

            LoadMenuMonAn();
        }

        private void LoadMenuMonAn()
        {
            if (wpMenu == null) return;

            using (var db = new MilkTeaDBContext())
            {
                var dsMonAn = db.Items.Where(i => i.isAvailable == true).ToList();

                if (_currentCategory != "Tất cả")
                {
                    string dbItemType = "";

                    if (_currentCategory == "Trà Sữa")
                    {
                        dbItemType = "Milk Tea";
                    }
                    else if (_currentCategory == "Trà Trái Cây")
                    {
                        dbItemType = "Fruit Tea";
                    }

                    dsMonAn = dsMonAn.Where(i => i.itemType == dbItemType).ToList();
                }

                string keyword = txtTimKiem?.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(keyword))
                {
                    dsMonAn = dsMonAn.Where(i => i.itemName.ToLower().Contains(keyword)).ToList();
                }

                wpMenu.Children.Clear();
                foreach (var mon in dsMonAn)
                {
                    System.Windows.Controls.Button btnMon = new System.Windows.Controls.Button
                    {
                        Width = 160,
                        Height = 100,
                        Margin = new Thickness(5),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = mon,
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                        BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 231, 235)),
                        BorderThickness = new Thickness(1)
                    };

                    StackPanel sp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    sp.Children.Add(new TextBlock { Text = mon.itemName, FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });
                    sp.Children.Add(new TextBlock { Text = $"Size {mon.size}", Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray), FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 5) });
                    sp.Children.Add(new TextBlock { Text = $"{mon.price:N0} đ", Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(242, 133, 0)), FontWeight = FontWeights.Heavy, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });

                    btnMon.Content = sp;
                    btnMon.Click += BtnMonAn_Click;
                    wpMenu.Children.Add(btnMon);
                }
            }
        }

        private void BtnMonAn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag is Item monDuocChon)
            {
                wTuyChinhPOS formTuyChinh = new wTuyChinhPOS(monDuocChon);
                if (formTuyChinh.ShowDialog() == true)
                {
                    string ghiChu = formTuyChinh.Note;

                    var monTrongGio = _gioHang.FirstOrDefault(x => x.itemID == monDuocChon.itemID && x.size == monDuocChon.size && x.note == ghiChu);

                    if (monTrongGio != null)
                    {
                        monTrongGio.quantity++;
                    }
                    else
                    {
                        _gioHang.Add(new POSCartItem
                        {
                            itemID = monDuocChon.itemID,
                            itemName = monDuocChon.itemName,
                            size = monDuocChon.size,
                            price = monDuocChon.price,
                            note = ghiChu,
                            quantity = 1
                        });
                    }
                    CapNhatGioHang();
                }
            }
        }

        private void btnXoaMon_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
            {
                int idCanXoa = (int)btn.Tag;
                var monCanXoa = _gioHang.FirstOrDefault(x => x.itemID == idCanXoa);
                if (monCanXoa != null)
                {
                    _gioHang.Remove(monCanXoa);
                    CapNhatGioHang();
                }
            }
        }

        private void CapNhatGioHang()
        {
            if (dgGioHang == null || txtTongTien == null)
            {
                return;
            }

            dgGioHang.ItemsSource = null;
            dgGioHang.ItemsSource = _gioHang;
            double tongTien = _gioHang.Sum(x => x.total);
            txtTongTien.Text = $"{tongTien:N0} đ";
        }

        private void rdoLoaiKhach_Checked(object sender, RoutedEventArgs e)
        {
            if (gridKhachHang == null || txtSdtKhach == null || txtTenKhachHang == null) return;

            if (rdoKhachThanhVien != null && rdoKhachThanhVien.IsChecked == true)
            {
                gridKhachHang.IsEnabled = true; gridKhachHang.Opacity = 1.0;
                txtTenKhachHang.Text = "Vui lòng nhập SĐT để kiểm tra...";
                txtTenKhachHang.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else if (rdoKhachLe != null && rdoKhachLe.IsChecked == true)
            {
                gridKhachHang.IsEnabled = false; gridKhachHang.Opacity = 0.5;
                txtSdtKhach.Clear(); _khachHangHienTai = null;
                txtTenKhachHang.Text = "Khách lẻ (Không tích điểm)";
                txtTenKhachHang.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void btnTimKhach_Click(object sender, RoutedEventArgs e)
        {
            string sdt = txtSdtKhach.Text.Trim();
            UserService userService = new UserService();
            _khachHangHienTai = userService.GetUserByPhone(sdt);

            if (_khachHangHienTai != null)
            {
                txtTenKhachHang.Text = $"Khách hàng hợp lệ (SĐT: {_khachHangHienTai.Phone})";
                txtTenKhachHang.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else
            {
                txtTenKhachHang.Text = "❌ Không tìm thấy khách hàng này!";
                txtTenKhachHang.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                _khachHangHienTai = null;
            }
        }

        private void btnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (_gioHang.Count == 0)
            {
                System.Windows.MessageBox.Show("Giỏ hàng đang trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentStaffId = GetCurrentStaffId();
            if (currentStaffId <= 0)
            {
                System.Windows.MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên! Vui lòng đăng nhập lại.", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            double tongTien = _gioHang.Sum(x => x.total);
            int? customerId = null;

            if (rdoKhachThanhVien.IsChecked == true && _khachHangHienTai != null)
            {
                customerId = _khachHangHienTai.userID;
            }
            else
            {
                UserService us = new UserService();
                var guest = us.GetUserByPhone("0000000000");
                if (guest != null) customerId = guest.userID;
            }

            OrderManager orderManager = new OrderManager();
            int newOrderId = orderManager.CreateNewOrder(currentStaffId, customerId, _gioHang, tongTien);

            if (newOrderId > 0)
            {
                int diemCongTamtinh = customerId != null ? (int)(tongTien / 10000) : 0;
                wHoaDon formHoaDon = new wHoaDon(newOrderId, $"{tongTien:N0} đ", diemCongTamtinh, 0, _gioHang);
                formHoaDon.ShowDialog();

                _gioHang.Clear();
                CapNhatGioHang();
                rdoKhachLe.IsChecked = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Đã xảy ra lỗi khi tạo hóa đơn!", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}