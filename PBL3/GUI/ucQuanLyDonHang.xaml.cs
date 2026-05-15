using PBL3.Data;
using PBL3.Interface;
using PBL3.Manangers;
using PBL3.Core;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;

namespace PBL3.GUI
{
    public partial class ucQuanLyDonHang : System.Windows.Controls.UserControl
    {
        public ucQuanLyDonHang()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            OrderManager manager = new OrderManager();
            dgOrders.ItemsSource = manager.GetAllOrders();
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

            using (var db = new MilkTeaDBContext())
            {
                var query = db.Orders.AsQueryable();
                if (status != "Tất cả" && !string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.orderStatus == status);
                }
                dgOrders.ItemsSource = query.OrderByDescending(o => o.orderID).ToList();
            }
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button? btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
            {
                int orderId = (int)btn.Tag;

                wChiTietDon detailWindow = new wChiTietDon(orderId, canApprove: false);
                detailWindow.ShowDialog();

                LoadOrders();
            }
        }
        private void btnApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nhân viên có đang check-in không
            StaffManager staffManager = new StaffManager();
            var (isCheckedIn, _, _, _) = staffManager.GetCheckOutStatus(UserSession.CurrentUser.userID);

            if (!isCheckedIn)
            {
                System.Windows.MessageBox.Show("❌ Bạn chưa check-in. Vui lòng check-in trước khi xử lí đơn!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            OrderService orderService = new OrderService();
            var nextOrder = orderService.GetNextOrder();
            if (nextOrder != null)
            {
                wChiTietDon detailWindow = new wChiTietDon(nextOrder.orderID, canApprove: true);
                detailWindow.ShowDialog();

                LoadOrders();
            }
        }
    }
}