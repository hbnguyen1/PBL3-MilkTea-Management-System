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

        // ✅ THÊM BIẾN ĐẾM: Giúp giảm tần suất chọc vào Database
        private int _reminderCheckCounter = 0;

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
            // Đồng hồ hiển thị thời gian thực vẫn chạy mỗi giây cực mượt
            txtTime.Text = DateTime.Now.ToString("HH:mm:ss");
            txtDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            // ✅ TỐI ƯU: Chỉ kiểm tra nhắc nhở hết ca mỗi 30 giây (30 ticks) thay vì mỗi giây
            _reminderCheckCounter++;
            if (_reminderCheckCounter >= 30)
            {
                _reminderCheckCounter = 0;
                CheckAndShowCheckOutReminder();
            }
        }

        private void CheckAndShowCheckOutReminder()
        {
            int staffId = GetCurrentStaffId();
            if (staffId <= 0) return;

            if (_staffService.ShouldShowCheckOutReminder(staffId))
            {
                string reminder = _staffService.GetCheckOutReminder(staffId);

                if (txtReminder != null && txtReminder.Text != reminder)
                {
                    txtReminder.Text = reminder;
                    txtReminder.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.OrangeRed);

                    System.Windows.MessageBox.Show(reminder, "⏰ CẢNH BÁO HẾT CA", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
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

        // ✅ ĐÃ VÁ LỖI: Sắp xếp lại cấu trúc try-catch chuẩn chỉnh cho nút đăng ký ca
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

            if (string.IsNullOrEmpty(selectedShift))
            {
                System.Windows.MessageBox.Show("Vui lòng chọn ca làm!");
                return;
            }

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

        // Các hàm helper được đưa về đúng vị trí trong Class
        public int GetRemainingSpots(DateTime workDate, string shift)
        {
            return _staffService.GetRemainingSpots(workDate, shift);
        }

        public int GetRegisteredStaffCount(DateTime workDate, string shift)
        {
            return _staffService.GetRegisteredStaffCountForShift(workDate, shift);
        }
    }
}