using System.Windows;
using System.Windows.Controls;
using PBL3.Manangers;

namespace PBL3.GUI
{
    public partial class ucQuanLyMon : System.Windows.Controls.UserControl
    {
        private ItemManager _itemManager = new ItemManager();

        public ucQuanLyMon()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            dgMonAn.ItemsSource = _itemManager.GetAllMenuItems();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnThemMonMoi_Click(object sender, RoutedEventArgs e)
        {
            wThietLapMon formSetup = new wThietLapMon(-1);
            formSetup.ShowDialog();
            LoadData();
        }

        private void btnMoFormSua_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
            {
                int itemId = (int)btn.Tag;
                wThietLapMon formSetup = new wThietLapMon(itemId);
                formSetup.ShowDialog();
                LoadData();
            }
        }
    }
}