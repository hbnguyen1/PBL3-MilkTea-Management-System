using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Application.Interface;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PBL3.UI.Views
{
    public partial class ucBossQuanLyKho : System.Windows.Controls.UserControl
    {
        private readonly IImportService _importService;
        private List<ImportNote> _allImports;
        private ImportNote _selectedImport;

        public ucBossQuanLyKho()
        {
            InitializeComponent();
            _importService = Program.ServiceProvider.GetRequiredService<IImportService>();
            LoadData();
            SetupDatePickers();
        }

        private void SetupDatePickers()
        {
            dtpDenNgay.SelectedDate = DateTime.Now;
            dtpTuNgay.SelectedDate = DateTime.Now.AddMonths(-1);
        }

        private void LoadData()
        {
            _allImports = _importService.GetAllImports();
            dgLichSuNhap.ItemsSource = _allImports;
            dgDanhSachPhieu.ItemsSource = _allImports;
        }

        private void btnTabLichSu_Click(object sender, RoutedEventArgs e)
        {
            ShowTabLichSu();
        }

        private void btnTabInHoaDon_Click(object sender, RoutedEventArgs e)
        {
            ShowTabInHoaDon();
        }

        private void ShowTabLichSu()
        {
            pnLichSu.Visibility = Visibility.Visible;
            pnInHoaDon.Visibility = Visibility.Collapsed;

            btnTabLichSu.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));
            btnTabInHoaDon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128));

            if (underlineLichSu != null) underlineLichSu.Visibility = Visibility.Visible;
            if (underlineInHoaDon != null) underlineInHoaDon.Visibility = Visibility.Collapsed;
        }

        private void ShowTabInHoaDon()
        {
            pnLichSu.Visibility = Visibility.Collapsed;
            pnInHoaDon.Visibility = Visibility.Visible;

            btnTabLichSu.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128));
            btnTabInHoaDon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));

            if (underlineLichSu != null) underlineLichSu.Visibility = Visibility.Collapsed;
            if (underlineInHoaDon != null) underlineInHoaDon.Visibility = Visibility.Visible;
        }

        private void rbLichSu_Checked(object sender, RoutedEventArgs e)
        {
            if (pnLichSu == null || pnInHoaDon == null) return;
            pnLichSu.Visibility = Visibility.Visible;
            pnInHoaDon.Visibility = Visibility.Collapsed;
        }

        private void rbInHoaDon_Checked(object sender, RoutedEventArgs e)
        {
            pnLichSu.Visibility = Visibility.Collapsed;
            pnInHoaDon.Visibility = Visibility.Visible;
        }

        private void txtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void dtpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterData();
        }

        private void dtpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            if (_allImports == null) return;

            var filtered = _allImports.AsEnumerable();

            if (dtpTuNgay.SelectedDate.HasValue && dtpDenNgay.SelectedDate.HasValue)
            {
                DateTime startDate = dtpTuNgay.SelectedDate.Value;
                DateTime endDate = dtpDenNgay.SelectedDate.Value;
                filtered = filtered.Where(x => x.importDate.Date >= startDate.Date && x.importDate.Date <= endDate.Date);
            }

            string searchText = txtTimKiem.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(x =>
                    x.importID.ToString().Contains(searchText) ||
                    x.staffID.ToString().Contains(searchText)
                );
            }

            dgLichSuNhap.ItemsSource = filtered.ToList();
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtTimKiem.Clear();
            SetupDatePickers();
            LoadData();
        }

        private void dgLichSuNhap_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgLichSuNhap.SelectedItem is ImportNote import)
            {
                ShowImportDetails(import);
            }
        }

        private void dgDanhSachPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDanhSachPhieu.SelectedItem is ImportNote import)
            {
                _selectedImport = import;
                ShowImportDetails(import);
            }
        }

        private void ShowImportDetails(ImportNote import)
        {
            lblMaPhieu.Text = import.importID.ToString();
            lblNgayNhap.Text = import.importDate.ToString("dd/MM/yyyy HH:mm");
            lblNhanVien.Text = import.staffID.ToString();
            lblMucHang.Text = import.ImportDetails.Count.ToString();
            lblTongTien.Text = $"{import.totalCost:N0} đ";

            var details = import.ImportDetails.Select(x => new
            {
                x.importId,
                x.igId,
                x.quantityAdded,
                x.importPrice,
                Ingredient = x.Ingredient,
                ThanhTien = x.quantityAdded * x.importPrice
            }).ToList();

            dgChiTietPhieu.ItemsSource = details;
        }

        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedImport == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn phiếu nhập để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var chiTietList = new System.Collections.ObjectModel.ObservableCollection<wInPhieuNhapKho.ChiTietNhapKho>();

                foreach (var detail in _selectedImport.ImportDetails)
                {
                    chiTietList.Add(new wInPhieuNhapKho.ChiTietNhapKho
                    {
                        TenNguyenLieu = detail.Ingredient?.igName ?? "N/A",
                        DonVi = detail.Ingredient?.unit ?? "",
                        SoLuong = detail.quantityAdded,
                        DonGia = detail.importPrice,
                        ThanhTien = detail.quantityAdded * detail.importPrice
                    });
                }

                wInPhieuNhapKho windowIn = new wInPhieuNhapKho();
                windowIn.LoadData(
                    _selectedImport.importID,
                    _selectedImport.importDate,
                    _selectedImport.staffID,
                    chiTietList,
                    _selectedImport.totalCost
                );
                windowIn.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi mở phiếu in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}