using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;

namespace PBL3.UI.Views
{
    public partial class wInPhieuNhapKho : Window
    {
        public class ChiTietNhapKho
        {
            public string TenNguyenLieu { get; set; }
            public string DonVi { get; set; }
            public int SoLuong { get; set; }
            public int DonGia { get; set; }
            public int ThanhTien { get; set; }
        }

        public wInPhieuNhapKho()
        {
            InitializeComponent();
        }

        public void LoadData(int maPhieu, DateTime ngayNhap, int nhanVienId, ObservableCollection<ChiTietNhapKho> chiTiet, int tongChiPhi)
        {
            lblMaPhieu.Text = $"#{maPhieu:D4}";
            lblNgayNhap.Text = ngayNhap.ToString("dd/MM/yyyy • HH:mm");
            lblNhanVien.Text = $"NV#{nhanVienId:D3}";
            lblSoMuc.Text = $"{chiTiet.Count} mục";

            icChiTiet.ItemsSource = chiTiet;

            int tongTien = 0;
            foreach (var item in chiTiet)
            {
                tongTien += item.ThanhTien;
            }

            lblCongNguyenLieu.Text = $"{tongTien:N0} đ";
            lblTongChiPhi.Text = $"{tongChiPhi:N0} đ";
            lblTongMuc.Text = chiTiet.Count.ToString();
            lblTongTien.Text = $"{tongChiPhi:N0} đ";
            lblThoiGianIn.Text = $"Thời gian in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument doc = GeneratePrintDocument();
                    printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Phiếu Nhập Kho");

                    System.Windows.MessageBox.Show("In phiếu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi in phiếu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument GeneratePrintDocument()
        {
            FlowDocument doc = new FlowDocument();
            doc.PageHeight = 297;
            doc.PageWidth = 210;
            doc.Foreground = System.Windows.Media.Brushes.Black;
            doc.FontFamily = new System.Windows.Media.FontFamily("Calibri");

            // Title
            Paragraph title = new Paragraph(new Run("PHIẾU NHẬP KHO"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            doc.Blocks.Add(title);

            Paragraph companyInfo = new Paragraph()
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };
            companyInfo.Inlines.Add(new Run("TTN Milk Tea Management System\n") { FontSize = 11, FontWeight = FontWeights.Bold });
            companyInfo.Inlines.Add(new Run("Địa chỉ: 193 Nguyễn Lương Bằng, Liên Chiểu, Đà Nẵng\n") { FontSize = 10 });
            companyInfo.Inlines.Add(new Run("Điện thoại: 0866744125 ") { FontSize = 10 });
            doc.Blocks.Add(companyInfo);

            doc.Blocks.Add(new Paragraph(new Run(new string('─', 60)))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 5, 0, 15)
            });

            Table infoTable = new Table() { CellSpacing = 10 };
            infoTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            infoTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });

            TableRowGroup infoGroup = new TableRowGroup();

            TableRow row1 = new TableRow();
            row1.Cells.Add(new TableCell(new Paragraph(new Run("Mã phiếu:") { FontWeight = FontWeights.Bold })));
            row1.Cells.Add(new TableCell(new Paragraph(new Run(lblMaPhieu.Text))));
            infoGroup.Rows.Add(row1);

            TableRow row2 = new TableRow();
            row2.Cells.Add(new TableCell(new Paragraph(new Run("Ngày nhập:") { FontWeight = FontWeights.Bold })));
            row2.Cells.Add(new TableCell(new Paragraph(new Run(lblNgayNhap.Text))));
            infoGroup.Rows.Add(row2);

            TableRow row3 = new TableRow();
            row3.Cells.Add(new TableCell(new Paragraph(new Run("Nhân viên:") { FontWeight = FontWeights.Bold })));
            row3.Cells.Add(new TableCell(new Paragraph(new Run(lblNhanVien.Text))));
            infoGroup.Rows.Add(row3);

            infoTable.RowGroups.Add(infoGroup);
            doc.Blocks.Add(infoTable);

            doc.Blocks.Add(new Paragraph(new Run(new string('─', 60)))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 10)
            });

            Table itemsTable = new Table();
            itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(60) });
            itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            itemsTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });

            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            headerRow.Background = System.Windows.Media.Brushes.LightGray;

            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Nguyên liệu") { FontWeight = FontWeights.Bold })));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("SL") { FontWeight = FontWeights.Bold }) { TextAlignment = TextAlignment.Right }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Đơn giá") { FontWeight = FontWeights.Bold }) { TextAlignment = TextAlignment.Right }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Thành tiền") { FontWeight = FontWeights.Bold }) { TextAlignment = TextAlignment.Right }));

            headerGroup.Rows.Add(headerRow);
            itemsTable.RowGroups.Add(headerGroup);

            TableRowGroup itemGroup = new TableRowGroup();
            if (icChiTiet.ItemsSource is ObservableCollection<ChiTietNhapKho> items)
            {
                foreach (var item in items)
                {
                    TableRow itemRow = new TableRow();
                    itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"{item.TenNguyenLieu} ({item.DonVi})"))));
                    itemRow.Cells.Add(new TableCell(new Paragraph(new Run(item.SoLuong.ToString())) { TextAlignment = TextAlignment.Right }));
                    itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"{item.DonGia:N0}")) { TextAlignment = TextAlignment.Right }));
                    itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"{item.ThanhTien:N0}")) { TextAlignment = TextAlignment.Right }));
                    itemGroup.Rows.Add(itemRow);
                }
            }
            itemsTable.RowGroups.Add(itemGroup);
            doc.Blocks.Add(itemsTable);

            doc.Blocks.Add(new Paragraph(new Run(new string('─', 60)))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            });

            Paragraph totalPara = new Paragraph()
            {
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 10, 0, 20)
            };
            totalPara.Inlines.Add(new Run($"TỔNG CHI PHÍ: {lblTongChiPhi.Text}")
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold
            });
            doc.Blocks.Add(totalPara);

            Paragraph footer = new Paragraph()
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            footer.Inlines.Add(new Run("Xin cảm ơn quý công ty!\n") { FontStyle = FontStyles.Italic });
            footer.Inlines.Add(new Run(lblThoiGianIn.Text));
            doc.Blocks.Add(footer);

            return doc;
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}