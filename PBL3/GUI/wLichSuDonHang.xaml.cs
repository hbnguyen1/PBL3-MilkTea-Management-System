using System.Linq;
using System.Windows;
using PBL3.Data; 

namespace PBL3.GUI
{
    public partial class wLichSuDonHang : Window
    {
        private int _customerId;

        public wLichSuDonHang(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;

            LoadLichSu();
        }

        private void LoadLichSu()
        {
            using (var db = new MilkTeaDBContext())
            {
                var history = db.Orders
                                .Where(o => o.customerID == _customerId)
                                .OrderByDescending(o => o.orderDate)
                                .ToList();

                if (history.Count == 0)
                {
                    System.Windows.MessageBox.Show("Bạn chưa có đơn hàng nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    dgLichSu.ItemsSource = history;
                }
            }
        }
    }
}