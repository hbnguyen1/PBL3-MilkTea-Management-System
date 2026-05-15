using Microsoft.Extensions.DependencyInjection;
using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;
using PBL3.Service;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PBL3.GUI
{
    public partial class ucQuanLyDonHang : System.Windows.Controls.UserControl
    {
        private readonly IOrderService _orderService;
        public ucQuanLyDonHang()
        {
            InitializeComponent();
            _orderService = Program.ServiceProvider.GetRequiredService<IOrderService>();
            LoadOrders();
        }

        private void LoadOrders()
        {
            dgOrders.ItemsSource = _orderService.GetAllOrders();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void cbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders == null) return;

            string? status = (cbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
            status = status ?? "Tất cả";
            if (status != "Tất cả" && !string.IsNullOrEmpty(status))
            {
                dgOrders.ItemsSource = _orderService.GetOrdersByStatus(status);
            }
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button? btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
            {
                int orderId = (int)btn.Tag;

                wChiTietDon detailWindow = new wChiTietDon(orderId);
                detailWindow.ShowDialog();

                LoadOrders();
            }
        }
        private void btnApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button? btn = sender as System.Windows.Controls.Button;
            var nextOrder = _orderService.GetNextOrder();
            if (nextOrder != null)
            {
                wChiTietDon detailWindow = new wChiTietDon(nextOrder.orderID);
                detailWindow.ShowDialog();

                LoadOrders();
            }
        }
    }
}