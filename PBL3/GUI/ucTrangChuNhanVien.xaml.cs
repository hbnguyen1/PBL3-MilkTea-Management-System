using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using PBL3.Core;
using PBL3.Interface;
using PBL3.Service;

namespace PBL3.GUI
{
    public partial class ucTrangChuNhanVien : System.Windows.Controls.UserControl
    {
        private DispatcherTimer _timer;
        private readonly IStaffService _staffService;

        public ucTrangChuNhanVien()
        {
            InitializeComponent();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();

            StartClock();

            dpNgayDangKy.SelectedDate = DateTime.Today;

            LoadSchedule();
        }

        private int GetCurrentStaffId()
        {
            if (UserSession.CurrentUser?.userID > 0)
            {
                return UserSession.CurrentUser.userID;
            }
            return 0; 
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtTime.Text = DateTime.Now.ToString("HH:mm:ss");
            txtDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            // Kiểm tra và hiển thị cảnh báo check-out
            CheckAndShowCheckOutReminder();
        }

        private void CheckAndShowCheckOutReminder()
        {
            int staffId = GetCurrentStaffId();
            if (staffId <= 0) return;

            // Kiểm tra xem có cần hiển thị cảnh báo không
            if (_staffService.ShouldShowCheckOutReminder(staffId))
            {
                string reminder = _staffService.GetCheckOutReminder(staffId);

                // Chỉ hiển thị lần đầu (khi txtReminder.Text rỗng hoặc khác)
                if (txtReminder != null && txtReminder.Text != reminder)
                {
                    txtReminder.Text = reminder;
                    txtReminder.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.OrangeRed);

                    // Hiển thị popup cảnh báo
                    System.Windows.MessageBox.Show(reminder, "⏰ CẢNH BÁO HẾT CA", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                // Xóa cảnh báo khi không cần nữa
                if (txtReminder != null && !string.IsNullOrEmpty(txtReminder.Text))
                {
                    txtReminder.Text = "";
                }
            }
        }

        private void LoadSchedule()
        {
            int staffId = GetCurrentStaffId();
            if (staffId <= 0)
            {
                System.Windows.MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            dgLich.ItemsSource = _staffService.GetMyWeeklySchedule(staffId);
        }


        private void btnChamCong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int staffId = GetCurrentStaffId();
                if (staffId <= 0)
                {
                    System.Windows.MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên! Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string resultMessage = _staffService.ToggleShift(staffId);

                if (resultMessage.Contains("❌") || resultMessage.Contains("⚠"))
                {
                    System.Windows.MessageBox.Show(resultMessage, "Thông báo Chấm Công", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    System.Windows.MessageBox.Show(resultMessage, "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDangKyCa_Click(object sender, RoutedEventArgs e)
        {
            if (dpNgayDangKy.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn ngày muốn đăng ký!");
                return;
            }

            int staffId = GetCurrentStaffId();
            if (staffId <= 0)
            {
                System.Windows.MessageBox.Show("Lỗi: Không tìm thấy thông tin nhân viên! Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime selectedDate = dpNgayDangKy.SelectedDate.Value;
            string selectedShift = cmbCaLam.Text;

            try
            {
                string msg = _staffService.QuickRegisterShift(staffId, selectedDate, selectedShift);

                System.Windows.MessageBox.Show(msg, "Thông báo");

                if (msg.Contains("✔"))
                {
                    LoadSchedule();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}