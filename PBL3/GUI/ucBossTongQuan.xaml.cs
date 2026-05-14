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

            for (int i = 2020; i <= 2030; i++) cmbNam.Items.Add(i.ToString());
            cmbNam.SelectedItem = DateTime.Now.Year.ToString();

            for (int i = 1; i <= 12; i++) cmbKyBaoCao.Items.Add($"Tháng {i}");
            for (int i = 1; i <= 4; i++) cmbKyBaoCao.Items.Add($"Quý {i}");
            cmbKyBaoCao.SelectedIndex = DateTime.Now.Month - 1;

            LoadThongKe();
        }

        private void btnTraCuu_Click(object sender, RoutedEventArgs e) => LoadThongKe();

        private void LocDuLieu_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbKyBaoCao?.SelectedItem != null && cmbNam?.SelectedItem != null) LoadThongKe();
        }

        private void LoadThongKe()
        {
            try
            {
                string selectedKy = cmbKyBaoCao.SelectedItem.ToString();
                int year = int.Parse(cmbNam.SelectedItem.ToString());

                double doanhThu = 0, chiPhiNL = 0, chiPhiLuongMB = 0, loiNhuan = 0;

                if (selectedKy.StartsWith("Tháng"))
                {
                    int month = int.Parse(selectedKy.Replace("Tháng ", ""));
                    doanhThu = _revenueService.GetRevenueByMonth(month, year);

                    DateTime start = new DateTime(year, month, 1);
                    DateTime end = start.AddMonths(1).AddDays(-1);
                    chiPhiNL = _profitService.CalculateTotalIngredientCost(start, end);

                    double luong = _profitService.CalculateTotalSalaryCost(month, year);
                    chiPhiLuongMB = luong + 3000000;
                    loiNhuan = _profitService.GetProfitByMonth(month, year);
                    txtGhiChuChiPhi.Text = "Phí cố định 3Tr + Lương NV";
                }
                else if (selectedKy.StartsWith("Quý"))
                {
                    int quarter = int.Parse(selectedKy.Replace("Quý ", ""));
                    DateTime qStart = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                    DateTime qEnd = qStart.AddMonths(3).AddDays(-1);

                    doanhThu = _revenueService.GetRevenueByRange(qStart, qEnd);
                    chiPhiNL = _profitService.CalculateTotalIngredientCost(qStart, qEnd);

                    double luongQ = _profitService.CalculateQuarterTotalSalaryCost(quarter, year);
                    chiPhiLuongMB = luongQ + 9000000;
                    loiNhuan = _profitService.GetProfitByQuarter(quarter, year);
                    txtGhiChuChiPhi.Text = "Phí cố định 9Tr + Lương NV";
                }

                txtDoanhThu.Text = $"{doanhThu:N0} đ";
                txtChiPhiNL.Text = $"{chiPhiNL:N0} đ";
                txtChiPhiLuong.Text = $"{chiPhiLuongMB:N0} đ";
                txtLoiNhuan.Text = $"{loiNhuan:N0} đ";
                txtLoiNhuan.Foreground = new SolidColorBrush(loiNhuan < 0 ? Colors.Red : System.Windows.Media.Color.FromRgb(16, 185, 129));

                VeBieuDoTheoNam(year);
            }
            catch { }
        }

        private void VeBieuDoTheoNam(int year)
        {
            ChartValues<double> valuesDT = new ChartValues<double>();
            ChartValues<double> valuesLN = new ChartValues<double>();

            for (int i = 1; i <= 12; i++)
            {
                valuesDT.Add(_revenueService.GetRevenueByMonth(i, year));
                valuesLN.Add(_profitService.GetProfitByMonth(i, year));
            }

            if (BieuDoDoanhThu == null) BieuDoDoanhThu = new SeriesCollection();
            BieuDoDoanhThu.Clear();

            BieuDoDoanhThu.Add(new ColumnSeries
            {
                Title = "Doanh thu",
                Values = valuesDT,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246)),
                MaxColumnWidth = 15
            });

            BieuDoDoanhThu.Add(new ColumnSeries
            {
                Title = "Lợi nhuận",
                Values = valuesLN,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)),
                MaxColumnWidth = 15
            });
        }
    }
}
