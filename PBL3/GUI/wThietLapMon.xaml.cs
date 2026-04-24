using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Manangers;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.GUI
{
    public partial class wThietLapMon : Window
    {
        private int _itemId = -1;
        private List<Recipe> _tempRecipes = new List<Recipe>();
        private ItemManager _itemManager = new ItemManager();
        private IngredientService _igService = new IngredientService();

        public wThietLapMon(int itemId)
        {
            InitializeComponent();
            _itemId = itemId;

            cmbNguyenLieu.ItemsSource = _igService.GetAllIngredients();

            if (_itemId != -1)
            {
                this.Title = "Cập nhật Sản Phẩm";
                LoadExistingData();
            }
        }

        private void LoadExistingData()
        {
            var item = _itemManager.GetItem(_itemId);
            if (item != null)
            {
                txtTenMon.Text = item.itemName;
                cmbLoai.Text = item.itemType;
                txtGiaBase.Text = item.price.ToString();
                chkAvailable.IsChecked = item.isAvailable;

                using (var db = new PBL3.Data.MilkTeaDBContext())
                {
                    _tempRecipes = db.Recipes.Where(r => r.itemID == _itemId && r.size == "M").ToList();
                    dgCongThuc.ItemsSource = _tempRecipes;
                }
            }
        }

        private void btnThemNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            if (cmbNguyenLieu.SelectedValue == null) return;

            int igId = (int)cmbNguyenLieu.SelectedValue;

            if (!int.TryParse(txtDinhLuong.Text, out int quantity))
            {
                System.Windows.MessageBox.Show("Định lượng phải là số nguyên hợp lệ!");
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

        private void btnXoaNguyenLieuKhoiCT_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
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
                    System.Windows.MessageBox.Show("Món ăn phải có ít nhất 1 nguyên liệu trong công thức!");
                    return;
                }

                if (_itemId == -1)
                {
                    _itemManager.AddItem(name, type, basePrice, _tempRecipes, isAvail);
                    System.Windows.MessageBox.Show("Thêm món và công thức thành công!");
                }
                else
                {
                    _itemManager.UpdateItem(_itemId, "M", name, type, basePrice, isAvail);
                    _itemManager.UpdateItem(_itemId, "L", name, type, basePrice + 5000, isAvail);

                    using (var db = new PBL3.Data.MilkTeaDBContext())
                    {
                        var oldRecipes = db.Recipes.Where(r => r.itemID == _itemId).ToList();
                        db.Recipes.RemoveRange(oldRecipes);

                        List<Recipe> newRecipes = new List<Recipe>();
                        foreach (var r in _tempRecipes)
                        {
                            newRecipes.Add(new Recipe { itemID = _itemId, size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                            newRecipes.Add(new Recipe { itemID = _itemId, size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
                        }
                        db.Recipes.AddRange(newRecipes);
                        db.SaveChanges();
                    }
                    System.Windows.MessageBox.Show("Cập nhật thông tin thành công!");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }
    }
}