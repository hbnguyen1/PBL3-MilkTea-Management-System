using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3.Interface;
using LiveCharts; 
using LiveCharts.Wpf;
using System.Collections.Generic;

namespace PBL3.GUI
{
    public partial class ucBossTongQuan : System.Windows.Controls.UserControl
    {
        private RevenueService _revenueService = new RevenueService();
        private ProfitService _profitService = new ProfitService();

        public SeriesCollection BieuDoDoanhThu { get; set; }
        public string[] ThangLabels { get; set; }
        public Func<double, string> FormatterTien { get; set; }

        public ucBossTongQuan()
        {
            BieuDoDoanhThu = new SeriesCollection();
            ThangLabels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };
            FormatterTien = value => value.ToString("N0");

            DataContext = this;

            InitializeComponent();
            for (int i = 2020; i <= 2030; i++)
            {
                cmbNam.Items.Add(i.ToString());
            }
            cmbThang.SelectedIndex = DateTime.Now.Month - 1;
            cmbNam.SelectedItem = DateTime.Now.Year.ToString();

            LoadThongKe();
        }

        private void btnTraCuu_Click(object sender, RoutedEventArgs e)
        {
            LoadThongKe();
        }

        private void LocDuLieu_Changed(object sender, RoutedEventArgs e)
        {
            if (cmbThang != null && cmbThang.SelectedItem != null && cmbNam != null && cmbNam.SelectedItem != null)
            {
                LoadThongKe();
            }
        }

        private void LoadThongKe()
        {
            try
            {
                int month = int.Parse((cmbThang.SelectedItem as ComboBoxItem).Content.ToString());
                int year = int.Parse(cmbNam.SelectedItem.ToString());

                double doanhThu = _revenueService.GetRevenueByMonth(month, year);
                txtDoanhThu.Text = $"{doanhThu:N0} đ";

                DateTime start = new DateTime(year, month, 1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                int chiPhiNguyenLieu = _profitService.CalculateTotalIngredientCost(start, end);
                txtChiPhiNL.Text = $"{chiPhiNguyenLieu:N0} đ";

                double luongNV = _profitService.CalculateTotalSalaryCost(month, year);
                double chiPhiLuongMatBang = luongNV + 10000000;
                txtChiPhiLuong.Text = $"{chiPhiLuongMatBang:N0} đ";

                double loiNhuan = _profitService.GetProfitByMonth(month, year);
                txtLoiNhuan.Text = $"{loiNhuan:N0} đ";

                if (loiNhuan < 0)
                {
                    txtLoiNhuan.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                else
                {
                    txtLoiNhuan.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
                }

                VeBieuDoTheoNam(year);
            }
            catch (Exception)
            {
                
            }
        }

        private void VeBieuDoTheoNam(int year)
        {
            ChartValues<double> doanhThuCacThang = new ChartValues<double>();

            for (int i = 1; i <= 12; i++)
            {
                double dtThang = _revenueService.GetRevenueByMonth(i, year);
                doanhThuCacThang.Add(dtThang);
            }

            if (BieuDoDoanhThu == null) BieuDoDoanhThu = new SeriesCollection();

            BieuDoDoanhThu.Clear();
            BieuDoDoanhThu.Add(new ColumnSeries
            {
                Title = "Doanh thu",
                Values = doanhThuCacThang,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246)),
                MaxColumnWidth = 40,
                DataLabels = true,
                LabelPoint = point => point.Y > 0 ? point.Y.ToString("N0") : ""
            });
        }
    }
}