using Microsoft.Extensions.DependencyInjection;
using PBL3.Core;
using PBL3.Interface; 
using PBL3.Models;
using PBL3.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PBL3.GUI
{
    public partial class wChiTietDon : Window
    {
        private int _orderId;
        private bool _canApprove;

        private readonly IOrderService _orderService;
        private readonly IItemService _itemService;
        private readonly IStaffService _staffService;

        public wChiTietDon(int orderId, bool canApprove = false)
        {
            InitializeComponent();
            _orderId = orderId;
            _canApprove = canApprove;
            lblMaDon.Text = $"Đơn hàng: #{orderId:D4}";

            _orderService = Program.ServiceProvider.GetRequiredService<IOrderService>();
            _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            try
            {
                var details = _orderService.GetOrderDetails(_orderId);
                var allItems = _itemService.GetAllItems();

                List<OrderDetailViewModel> viewList = new List<OrderDetailViewModel>();

                if (details != null)
                {
                    foreach (var d in details)
                    {
                        var itemInfo = allItems.FirstOrDefault(i => i.itemID == d.itemID && i.size == (d.size ?? "M"));
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

                var order = _orderService.GetOrderById(_orderId);
                if (order != null && order.orderStatus == "Completed" || !_canApprove)
                {
                    btnDuyet.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải chi tiết đơn: " + ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnDuyet_Click(object sender, RoutedEventArgs e)
        {
            var (isCheckedIn, _, _, _) = _staffService.GetCheckOutStatus(UserSession.CurrentUser.userID);

            if (!isCheckedIn)
            {
                System.Windows.MessageBox.Show("❌ Bạn chưa check-in. Vui lòng check-in trước khi hoàn thành đơn!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            var result = System.Windows.MessageBox.Show($"Xác nhận đã pha chế xong đơn #{_orderId}?", "Xác nhận", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    bool isSuccess = _orderService.ProcessNextOrderInQueue(_orderId, UserSession.CurrentUser.userID);
                    if (isSuccess)
                    {
                        System.Windows.MessageBox.Show("Đã duyệt đơn thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        this.Close();
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