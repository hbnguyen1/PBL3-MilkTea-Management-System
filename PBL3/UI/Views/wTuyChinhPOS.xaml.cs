using System.Windows;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Domain.Models;
using PBL3.src.Application.Interface;

namespace PBL3.UI.Views;

public partial class wTuyChinhPOS : Window
{
    private readonly IItemService _itemService;
    public string Note { get; set; } = "";
    public string SelectedSize { get; set; } = "M";
    public double SelectedPrice { get; set; } = 0;

    private Item _originalItem;

    public wTuyChinhPOS(Item mon)
    {
        InitializeComponent();

        _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();
        _originalItem = mon;
        txtTenMon.Text = $"{mon.itemName}";
        SelectedSize = "M"; 
        SelectedPrice = GetPriceForSize("M", mon.itemID);
        UpdatePriceDisplay();
    }

    private double GetPriceForSize(string size, int itemID)
    {
        var item = _itemService.GetItemSize(itemID, size);
        return item?.price ?? 0;
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