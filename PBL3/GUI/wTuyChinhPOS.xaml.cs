using System.Windows;
using PBL3.Models;

namespace PBL3.GUI
{
    public partial class wTuyChinhPOS : Window
    {
        public string Note { get; set; } = "";

        public wTuyChinhPOS(Item mon)
        {
            InitializeComponent();
            txtTenMon.Text = $"{mon.itemName} (Size {mon.size})";
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