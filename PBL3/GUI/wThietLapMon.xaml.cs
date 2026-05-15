using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PBL3.Models;
using PBL3.Interface;
using MessageBox = System.Windows.MessageBox;

namespace PBL3.GUI
{
    public partial class wThietLapMon : Window
    {
        private int _itemId = -1;
        private List<Recipe> _tempRecipes = new List<Recipe>();
        private readonly IItemService _itemService;
        private readonly IIngredientService _igService;

        public wThietLapMon(int itemId)
        {
            InitializeComponent();
            _itemId = itemId;
            _itemService = Program.ServiceProvider.GetRequiredService<IItemService>();
            _igService = Program.ServiceProvider.GetRequiredService<IIngredientService>();

            cmbNguyenLieu.ItemsSource = _igService.GetAllIngredients();

            if (_itemId != -1)
            {
                this.Title = "Cập nhật Sản Phẩm";
                LoadExistingData();
            }
        }

        private void LoadExistingData()
        {
            var item = _itemService.GetItemById(_itemId);
            if (item != null)
            {
                txtTenMon.Text = item.itemName;
                cmbLoai.Text = item.itemType;
                txtGiaBase.Text = item.price.ToString();
                chkAvailable.IsChecked = item.isAvailable;
                _tempRecipes = _itemService.GetRecipesByItem(_itemId, "M");
                dgCongThuc.ItemsSource = _tempRecipes;
            }
        }

        private void btnThemNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbNguyenLieu.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn nguyên liệu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int igId = (int)cmbNguyenLieu.SelectedValue;

                if (!int.TryParse(txtDinhLuong.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Định lượng phải là số nguyên dương hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string donVi = "";
                if (cmbNguyenLieu.SelectedItem is Ingredient selectedIg)
                {
                    donVi = selectedIg.unit ?? "";
                }

                var existing = _tempRecipes.FirstOrDefault(r => r.ingredientID == igId);
                if (existing != null)
                {
                    existing.quantityNeeded += quantity;
                }
                else
                {
                    _tempRecipes.Add(new Recipe
                    {
                        itemID = _itemId == -1 ? 0 : _itemId,
                        ingredientID = igId,
                        quantityNeeded = quantity,
                        size = "M",
                        unitUsed = donVi
                    });
                }

                dgCongThuc.ItemsSource = null;
                dgCongThuc.ItemsSource = _tempRecipes;
                txtDinhLuong.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnXoaNguyenLieuKhoiCT_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null)
            {
                int igId = (int)btn.Tag;
                var itemToRemove = _tempRecipes.FirstOrDefault(r => r.ingredientID == igId);
                if (itemToRemove != null)
                {
                    _tempRecipes.Remove(itemToRemove);
                    dgCongThuc.ItemsSource = null;
                    dgCongThuc.ItemsSource = _tempRecipes;
                }
            }
        }

        private void btnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtTenMon.Text.Trim();
                string type = cmbLoai.Text;
                int basePrice = int.Parse(txtGiaBase.Text);
                bool isAvail = chkAvailable.IsChecked == true;

                if (_tempRecipes.Count == 0)
                {
                    MessageBox.Show("Món ăn phải có ít nhất 1 nguyên liệu trong công thức!");
                    return;
                }

                if (_itemId == -1) // THÊM MỚI
                {
                    int newId = _itemService.GetNextItemID();

                    List<Item> newItems = new List<Item>
                    {
                        new Item { itemID = newId, size = "M", itemName = name, itemType = type, price = basePrice, isAvailable = isAvail },
                        new Item { itemID = newId, size = "L", itemName = name, itemType = type, price = basePrice + 5000, isAvailable = isAvail }
                    };

                    List<Recipe> finalRecipes = new List<Recipe>();
                    foreach (var r in _tempRecipes)
                    {
                        finalRecipes.Add(new Recipe { itemID = newId, size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                        finalRecipes.Add(new Recipe { itemID = newId, size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
                    }

                    _itemService.AddItemWithRecipe(newItems, finalRecipes);
                    MessageBox.Show("Thêm món và công thức thành công!");
                }
                else
                {
                    var updatedMItem = new Item { itemID = _itemId, size = "M", itemName = name, itemType = type, price = basePrice, isAvailable = isAvail };
                    var updatedLItem = new Item { itemID = _itemId, size = "L", itemName = name, itemType = type, price = basePrice + 5000, isAvailable = isAvail };

                    List<Recipe> newRecipes = new List<Recipe>();
                    foreach (var r in _tempRecipes)
                    {
                        newRecipes.Add(new Recipe { itemID = _itemId, size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                        newRecipes.Add(new Recipe { itemID = _itemId, size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
                    }

                    _itemService.UpdateItemWithRecipe(_itemId, updatedMItem, updatedLItem, newRecipes);
                    MessageBox.Show("Cập nhật thông tin thành công!");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }
    }
}