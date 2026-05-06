using System.Windows;
using PBL3.Models;
using PBL3.Data;
using System.Linq;

namespace PBL3.GUI
{
    public partial class wTuyChinhPOS : Window
    {
        public string Note { get; set; } = "";
        public string SelectedSize { get; set; } = "M";
        public double SelectedPrice { get; set; } = 0;

        private Item _originalItem;

        public wTuyChinhPOS(Item mon)
        {
            InitializeComponent();
            _originalItem = mon;

            txtTenMon.Text = $"{mon.itemName}";
            SelectedSize = "M"; // Mặc định M
            SelectedPrice = GetPriceForSize("M", mon.itemID);
            UpdatePriceDisplay();
        }

        private double GetPriceForSize(string size, int itemID)
        {
            using (var db = new MilkTeaDBContext())
            {
                var item = db.Items.FirstOrDefault(i => i.itemID == itemID && i.size == size);
                return item?.price ?? 0;
            }
        }

        private void rdoSize_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton rdo = sender as System.Windows.Controls.RadioButton;
            if (rdo != null && rdo.IsChecked == true)
            {
                SelectedSize = rdo.Tag.ToString();
                SelectedPrice = GetPriceForSize(SelectedSize, _originalItem.itemID);
                UpdatePriceDisplay();
            }
        }

        private void UpdatePriceDisplay()
        {
            if (SelectedPrice > 0)
            {
                txtGia.Text = $"{SelectedPrice:N0} đ";
            }
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            string duong = $"{(int)sldDuong.Value}% Đường";
            string da = $"{(int)sldDa.Value}% Đá";

            Note = $"{duong}, {da}";

            string ghiChuPhu = txtGhiChu.Text.Trim();
            if (!string.IsNullOrEmpty(ghiChuPhu))
            {
                Note += $" | Ghi chú: {ghiChuPhu}";
            }

            this.DialogResult = true;
        }
    }
}