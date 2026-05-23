using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.GUI
{
    public partial class ucQuanLyNhanVien : System.Windows.Controls.UserControl
    {
        private readonly IStaffService _staffService;
        private dynamic _selectedStaff = null;

        public ucQuanLyNhanVien()
        {
            InitializeComponent();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();
            cmbThang.SelectedIndex = DateTime.Now.Month - 1;
            txtNam.Text = DateTime.Now.Year.ToString();
            LoadDanhSachNhanVien();
        }

        private void LoadDanhSachNhanVien()
        {
            dgNhanVien.ItemsSource = _staffService.GetAllStaffs();
        }

        private void btnThemNhanVien_Click(object sender, RoutedEventArgs e)
        {
            wThemNhanVien formThem = Program.ServiceProvider.GetRequiredService<wThemNhanVien>();
            if (formThem.ShowDialog() == true)
            {
                LoadDanhSachNhanVien();
            }
        }

        private void dgNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgNhanVien.SelectedItem != null)
            {
                _selectedStaff = dgNhanVien.SelectedItem;

                gridChiTiet.IsEnabled = true;
                gridChiTiet.Opacity = 1.0;

                try { txtTenNhanVien.Text = _selectedStaff.Name; } catch { txtTenNhanVien.Text = "Nhân viên #" + _selectedStaff.userID; }
                try { txtChucVu.Text = $"Chức vụ: {_selectedStaff.Role} | Lương: {_selectedStaff.salaryPerHour:N0}đ/h"; } catch { txtChucVu.Text = ""; }

                LoadDuLieuChamCong();
            }
        }

        private void LọcDuLieu_Changed(object sender, RoutedEventArgs e)
        {
            if (_selectedStaff != null && cmbThang.SelectedItem != null)
            {
                LoadDuLieuChamCong();
            }
        }

        private void LoadDuLieuChamCong()
        {
            if (_selectedStaff == null) return;

            int staffId = _selectedStaff.userID;
            int month = int.Parse((cmbThang.SelectedItem as ComboBoxItem).Content.ToString());
            int year = 2026;
            int.TryParse(txtNam.Text, out year);

            var logs = _staffService.GetShiftLogs(staffId, month, year);
            dgChamCong.ItemsSource = logs;

            double totalHours = logs.Sum(l => l.totalHours);
            int totalPenalty = logs.Sum(l => l.penalty);
            double luong = _staffService.CalculateSalary(staffId, month, year);

            txtTongGio.Text = $"{totalHours:F1}h";
            txtTienPhat.Text = $"{totalPenalty:N0}đ";
            txtLuongThuc.Text = $"{luong:N0}đ";

            bool isSaved = _staffService.IsSalarySaved(staffId, month, year);
            if (isSaved)
            {
                btnChotLuong.Content = "ĐÃ CHỐT LƯƠNG";
                btnChotLuong.Background = new SolidColorBrush(Colors.Gray);
                btnChotLuong.IsEnabled = false;
            }
            else
            {
                btnChotLuong.Content = "CHỐT LƯƠNG";
                btnChotLuong.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
                btnChotLuong.IsEnabled = true;
            }
        }

        private void btnChotLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStaff == null) return;

            int staffId = _selectedStaff.userID;
            int month = int.Parse((cmbThang.SelectedItem as ComboBoxItem).Content.ToString());
            int year = int.Parse(txtNam.Text);

            string resultMsg = _staffService.SaveSalary(staffId, month, year);

            if (resultMsg.Contains("✔"))
            {
                System.Windows.MessageBox.Show(resultMsg, "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDuLieuChamCong();
            }
            else
            {
                System.Windows.MessageBox.Show(resultMsg, "Cảnh Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSuaNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Staff staff)
            {
                wThemNhanVien formSua = Program.ServiceProvider.GetRequiredService<wThemNhanVien>();
                formSua.SetEditMode(staff);

                if (formSua.ShowDialog() == true)
                {
                    LoadDanhSachNhanVien();
                }
            }
        }

        private void btnXoaNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Staff staff)
            {
                var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên [{staff.Name}]?\nMọi lịch làm, chấm công và bảng lương liên quan sẽ bị xóa sạch!",
                                             "Xác nhận xóa nhân sự", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isSuccess = _staffService.DeleteStaff(staff.userID);
                    if (isSuccess)
                    {
                        System.Windows.MessageBox.Show("Xóa nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadDanhSachNhanVien();

                        if (_selectedStaff != null && _selectedStaff.userID == staff.userID)
                        {
                            _selectedStaff = null;
                            gridChiTiet.IsEnabled = false;
                            gridChiTiet.Opacity = 0.4;
                            txtTenNhanVien.Text = "Chọn một nhân viên để xem";
                            txtChucVu.Text = "Vui lòng chọn từ danh sách bên trái";
                            dgChamCong.ItemsSource = null;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Xóa nhân viên thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}