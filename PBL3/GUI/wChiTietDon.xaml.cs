using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PBL3.Data; 
using PBL3.Models;

namespace PBL3.GUI
{
    public partial class wChiTietDon : Window
    {
        private int _orderId;

        public wChiTietDon(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            lblMaDon.Text = $"Đơn hàng: #{orderId:D4}";
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            using (var db = new MilkTeaDBContext())
            {
                var details = db.OrderDetails.Where(d => d.orderID == _orderId).ToList();
                List<OrderDetailViewModel> viewList = new List<OrderDetailViewModel>();

                if (details != null)
                {
                    foreach (var d in details)
                    {
                        var itemInfo = db.Items.FirstOrDefault(i => i.itemID == d.itemID && i.size == (d.size ?? "M"));
                        string itemName = itemInfo != null ? itemInfo.itemName : "Món không xác định";

                        viewList.Add(new OrderDetailViewModel
                        {
                            TenMon = itemName,
                            Size = d.size,
                            SoLuong = d.quantity, 
                            GhiChu = !string.IsNullOrEmpty(d.note) ? $" {d.note}" : ""
                        });
                    }
                }
                icChiTiet.ItemsSource = viewList;
                var order = db.Orders.FirstOrDefault(o => o.orderID == _orderId);
                if (order != null && order.orderStatus == "Completed")
                {
                    btnDuyet.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show($"Xác nhận đã pha chế xong đơn #{_orderId}?", "Xác nhận", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new MilkTeaDBContext())
                    {
                        var order = db.Orders.FirstOrDefault(o => o.orderID == _orderId);
                        if (order != null)
                        {
                            order.orderStatus = "Completed";
                            db.SaveChanges();

                            System.Windows.MessageBox.Show("Đã duyệt đơn thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            this.Close();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Duyệt đơn thất bại. Không tìm thấy đơn hàng.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi khi duyệt đơn: " + ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    public class OrderDetailViewModel
    {
        public string? TenMon { get; set; }
        public string? Size { get; set; }
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }
    }
}