using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Models;
using PBL3.Core;
using PBL3.Service;
using PBL3.Interface;
using Microsoft.Extensions.DependencyInjection; 

namespace PBL3.GUI
{
    public class ImportDetailUI
    {
        public int igId { get; set; }
        public string igName { get; set; }
        public int quantityAdded { get; set; }
        public int importPrice { get; set; }
    }

    public partial class wNhapHang : Window
    {
        private int _staffId;
        private List<ImportDetailUI> _chiTietNhap = new List<ImportDetailUI>();

        private readonly IIngredientService _ingredientService;
        private readonly IImportService _importService;

        public wNhapHang()
        {
            InitializeComponent();

            _ingredientService = Program.ServiceProvider.GetRequiredService<IIngredientService>();
            _importService = Program.ServiceProvider.GetRequiredService<IImportService>();

            _staffId = UserSession.CurrentUser?.userID ?? 0;
            if (_staffId <= 0)
            {
                System.Windows.MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
            LoadIngredients();
        }

        private void LoadIngredients()
        {
            cmbNguyenLieu.ItemsSource = _ingredientService.GetAllIngredients();
        }

        private void btnThemVaoPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (cmbNguyenLieu.SelectedItem is Ingredient selectedIg)
            {
                if (!int.TryParse(txtSoLuong.Text, out int sl) || sl <= 0)
                {
                    System.Windows.MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!"); return;
                }

                int giaMacDinh = (int)selectedIg.price;

                var existingItem = _chiTietNhap.FirstOrDefault(x => x.igId == selectedIg.igID);
                if (existingItem != null)
                {
                    existingItem.quantityAdded += sl;
                }
                else
                {
                    _chiTietNhap.Add(new ImportDetailUI
                    {
                        igId = selectedIg.igID,
                        igName = selectedIg.igName,
                        quantityAdded = sl,
                        importPrice = giaMacDinh
                    });
                }

                RefreshDataGrid();
                txtSoLuong.Clear();
            }
            else
            {
                System.Windows.MessageBox.Show("Vui lòng chọn nguyên liệu!");
            }
        }

        private void btnXoaKhoiPhieu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.Tag != null)
            {
                int idXoa = (int)btn.Tag;
                var item = _chiTietNhap.FirstOrDefault(x => x.igId == idXoa);
                if (item != null)
                {
                    _chiTietNhap.Remove(item);
                    RefreshDataGrid();
                }
            }
        }

        private void RefreshDataGrid()
        {
            if (dgChiTietNhap != null)
            {
                dgChiTietNhap.ItemsSource = null;
                dgChiTietNhap.ItemsSource = _chiTietNhap;
            }
        }

        private void btnHoanTatNhap_Click(object sender, RoutedEventArgs e)
        {
            if (_chiTietNhap.Count == 0)
            {
                System.Windows.MessageBox.Show("Phiếu nhập đang trống!");
                return;
            }

            List<ImportDetail> detailsToSave = new List<ImportDetail>();
            foreach (var item in _chiTietNhap)
            {
                detailsToSave.Add(new ImportDetail
                {
                    igId = item.igId,
                    quantityAdded = item.quantityAdded,
                    importPrice = item.importPrice
                });
            }

            bool success = _importService.CreateImport(_staffId, detailsToSave);

            if (success)
            {
                System.Windows.MessageBox.Show("Nhập kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Nhập kho thất bại, vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}