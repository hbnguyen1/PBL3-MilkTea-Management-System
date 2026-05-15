using Microsoft.Extensions.DependencyInjection;
using PBL3.Interface;
using PBL3.Models;
using PBL3.Service;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PBL3.GUI
{
    public partial class ucQuanLyKho : System.Windows.Controls.UserControl
    {
        private readonly IIngredientService _ingredientService;
        private int _selectedIgId = -1; 

        public ucQuanLyKho()
        {
            InitializeComponent();
            _ingredientService = Program.ServiceProvider.GetRequiredService<IIngredientService>();
            LoadData();
        }

        private void LoadData()
        {
            dgKho.ItemsSource = _ingredientService.GetAllIngredients();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtTen.Text.Trim();
                string unit = cmbDonVi.Text;
                int count = int.Parse(txtSoLuong.Text);
                int price = int.Parse(txtGia.Text);


                Ingredient ig = new Ingredient()
                {
                    igName = name,
                    price = price,
                    unit = unit,
                    igCount = count
                };

                if (_ingredientService.AddIngredient(ig))
                {
                    System.Windows.MessageBox.Show("Thêm nguyên liệu thành công!", "Thông báo");
                    LoadData();
                    btnHuyChon_Click(null, null); 
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Vui lòng kiểm tra lại dữ liệu nhập vào!", "Lỗi");
            }
        }

        private void btnNhapHang_Click(object sender, RoutedEventArgs e)
        {
            wNhapHang formNhap = new wNhapHang();
            formNhap.ShowDialog();
            LoadData();
        }

        private void btnCanhBao_Click(object sender, RoutedEventArgs e)
        {
            dgKho.ItemsSource = _ingredientService.GetLowStockIngredients();
        }

        private void dgKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKho.SelectedItem is PBL3.Models.Ingredient ig)
            {
                _selectedIgId = ig.igID;
                txtTen.Text = ig.igName;
                txtSoLuong.Text = ig.igCount.ToString();
                txtGia.Text = ig.price.ToString();
                cmbDonVi.Text = ig.unit;

                txtSoLuong.IsEnabled = false;
                btnThem.IsEnabled = false;
                btnSua.IsEnabled = true;
            }
        }

        private void btnHuyChon_Click(object sender, RoutedEventArgs e)
        {
            _selectedIgId = -1;
            txtTen.Clear();
            txtSoLuong.Clear();
            txtGia.Clear();
            cmbDonVi.SelectedIndex = 0;

            txtSoLuong.IsEnabled = true;
            btnThem.IsEnabled = true;
            btnSua.IsEnabled = false;
            dgKho.SelectedItem = null;
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIgId == -1) return;

            try
            {
                string name = txtTen.Text?.Trim() ?? "";
                string unit = cmbDonVi.Text ?? "";

                if (string.IsNullOrWhiteSpace(name))
                {
                    System.Windows.MessageBox.Show("Vui lòng nhập tên nguyên liệu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtGia.Text, out int price) || price < 0)
                {
                    System.Windows.MessageBox.Show("Giá phải là số không âm hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                

                if (!(string.IsNullOrEmpty(name)) && (_ingredientService.updateIngredient(_selectedIgId, name, unit, price)))
                {
                    System.Windows.MessageBox.Show("Cập nhật thông tin nguyên liệu thành công!", "Thông báo");
                    LoadData();
                    btnHuyChon_Click(null, null);
                }
                else
                {
                    System.Windows.MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}