using PBL3.Data;
using PBL3.Manangers;
using System.Linq;
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

                wChiTietDon detailWindow = new wChiTietDon(orderId);
                detailWindow.ShowDialog();

                LoadOrders();
            }
        }
    }
}