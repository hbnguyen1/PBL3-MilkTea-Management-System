using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PBL3.Interface;
using PBL3.Models;
using PBL3.Service;

namespace PBL3.GUI
{
    public partial class ucQuanLyMon : System.Windows.Controls.UserControl
    {
        private readonly IItemService _itemService;

        public ucQuanLyMon()
        {
            InitializeComponent();
            _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();
            LoadData();
        }

        private void LoadData()
        {
            List<Item> allItems = new List<Item>();
            var milkTeas = _itemService.GetMenuByCategory("Milk Tea");
            if (milkTeas != null) allItems.AddRange(milkTeas);
            var fruitTeas = _itemService.GetMenuByCategory("Fruit Tea");
            if (fruitTeas != null) allItems.AddRange(fruitTeas);
            var topping = _itemService.GetMenuByCategory("Topping");
            if (topping != null) allItems.AddRange(topping);
            var others = _itemService.GetMenuByCategory("Món khác");
            if (others != null) allItems.AddRange(others);

            dgMonAn.ItemsSource = allItems.OrderBy(item => item.itemID).ToList();
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