using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.GUI
{
    public partial class wThietLapMon : Window
    {
        private int _itemId = -1;
        private List<Recipe> _tempRecipes = new List<Recipe>();
        private readonly IItemService _itemService;
        private readonly IIngredientService _igService;
        private string _selectedLocalFullPath = "";

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

        private void DisplayImage(string imgDbPath)
        {
            try
            {
                if (string.IsNullOrEmpty(imgDbPath)) return;

                // Cắt bỏ chữ "/Images/" đi để lấy tên file gốc
                string fileNameOnly = Path.GetFileName(imgDbPath);
                string projectFolder = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(projectFolder, "Images", fileNameOnly);

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.EndInit();
                    imgMonAn.Source = bitmap;
                }
            }
            catch { imgMonAn.Source = null; }
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

                DisplayImage(item.ImagePath);

                _tempRecipes = _itemService.GetRecipesByItem(_itemId, "M");
                dgCongThuc.ItemsSource = _tempRecipes;
            }
        }

        private void btnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openDialog.ShowDialog() == true)
            {
                _selectedLocalFullPath = openDialog.FileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(_selectedLocalFullPath);
                bitmap.EndInit();
                imgMonAn.Source = bitmap;
            }
        }

        private void btnThemNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbNguyenLieu.SelectedValue == null) return;
                int igId = (int)cmbNguyenLieu.SelectedValue;

                if (!int.TryParse(txtDinhLuong.Text, out int quantity) || quantity <= 0) return;

                string donVi = (cmbNguyenLieu.SelectedItem as Ingredient)?.unit ?? "";

                var existing = _tempRecipes.FirstOrDefault(r => r.ingredientID == igId);
                if (existing != null) existing.quantityNeeded += quantity;
                else
                {
                    _tempRecipes.Add(new Recipe { itemID = _itemId == -1 ? 0 : _itemId, ingredientID = igId, quantityNeeded = quantity, size = "M", unitUsed = donVi });
                }

                dgCongThuc.ItemsSource = null;
                dgCongThuc.ItemsSource = _tempRecipes;
                txtDinhLuong.Clear();
            }
            catch { }
        }

        private void btnXoaNguyenLieuKhoiCT_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null)
            {
                var itemToRemove = _tempRecipes.FirstOrDefault(r => r.ingredientID == (int)btn.Tag);
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
                    System.Windows.MessageBox.Show("Cần ít nhất 1 nguyên liệu!");
                    return;
                }

                if (_itemId == -1) // ==== THÊM MỚI ====
                {
                    int nextId = _itemService.GetNextItemID();

                    List<Item> newItems = new List<Item>
                    {
                        new Item { itemID = nextId, size = "M", itemName = name, itemType = type, price = basePrice, isAvailable = isAvail, ImagePath = "" },
                        new Item { itemID = nextId, size = "L", itemName = name, itemType = type, price = basePrice + 5000, isAvailable = isAvail, ImagePath = "" }
                    };

                    List<Recipe> finalRecipes = new List<Recipe>();
                    foreach (var r in _tempRecipes)
                    {
                        finalRecipes.Add(new Recipe { size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                        finalRecipes.Add(new Recipe { size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
                    }

                    imgMonAn.Source = null;
                    GC.Collect(); GC.WaitForPendingFinalizers();

                    bool saveResult = _itemService.AddItemWithRecipe(newItems, finalRecipes, _selectedLocalFullPath);

                    if (saveResult)
                    {
                        System.Windows.MessageBox.Show("Thành công!");
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                else // ==== CẬP NHẬT ====
                {
                    imgMonAn.Source = null;
                    GC.Collect(); GC.WaitForPendingFinalizers();

                    string targetDbPath = _itemService.GetItemById(_itemId)?.ImagePath ?? "";

                    if (!string.IsNullOrEmpty(_selectedLocalFullPath))
                    {
                        string ext = Path.GetExtension(_selectedLocalFullPath);
                        string fileNameOnly = $"mon_{_itemId}{ext}";
                        targetDbPath = $"/Images/{fileNameOnly}"; // ĐĂ SỬA: Đưa về chuẩn /Images/

                        // Chép ảnh vào Source Code
                        string projectFolder = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                        string sourceImgFolder = Path.Combine(projectFolder, "Images");
                        if (!Directory.Exists(sourceImgFolder)) Directory.CreateDirectory(sourceImgFolder);
                        File.Copy(_selectedLocalFullPath, Path.Combine(sourceImgFolder, fileNameOnly), true);

                        // Chép ảnh vào Runtime
                        string debugImgFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                        if (!Directory.Exists(debugImgFolder)) Directory.CreateDirectory(debugImgFolder);
                        File.Copy(_selectedLocalFullPath, Path.Combine(debugImgFolder, fileNameOnly), true);
                    }

                    var updatedMItem = new Item { itemID = _itemId, size = "M", itemName = name, itemType = type, price = basePrice, isAvailable = isAvail, ImagePath = targetDbPath };
                    var updatedLItem = new Item { itemID = _itemId, size = "L", itemName = name, itemType = type, price = basePrice + 5000, isAvailable = isAvail, ImagePath = targetDbPath };

                    List<Recipe> newRecipes = new List<Recipe>();
                    foreach (var r in _tempRecipes)
                    {
                        newRecipes.Add(new Recipe { itemID = _itemId, size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                        newRecipes.Add(new Recipe { itemID = _itemId, size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
                    }

                    _itemService.UpdateItemWithRecipe(_itemId, updatedMItem, updatedLItem, newRecipes);
                    System.Windows.MessageBox.Show("Cập nhật thành công!");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}