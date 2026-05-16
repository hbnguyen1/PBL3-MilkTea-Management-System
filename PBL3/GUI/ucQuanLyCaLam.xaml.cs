using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBL3.Data;
using WpfColor = System.Windows.Media.Color;

namespace PBL3.GUI
{
    public partial class ucQuanLyCaLam : System.Windows.Controls.UserControl
    {
        private DateTime currentWeekStart;
        // Dictionary để gom nhóm: Key là "Ngày_Ca", Value là Danh sách tên nhân viên
        private Dictionary<string, List<string>> weekScheduleData = new();

        // Khớp tên ca trong Database
        private List<string> dbShifts = new() { "Morning", "Afternoon", "Evening" };

        // Tên hiển thị ra giao diện cho đẹp
        private Dictionary<string, string> shiftDisplayNames = new()
        {
            { "Morning", "CA SÁNG (08:00 - 13:00)" },
            { "Afternoon", "CA CHIỀU (13:00 - 18:00)" },
            { "Evening", "CA TỐI (18:00 - 22:00)" }
        };

        // Màu nền pastel phân biệt cho các ca
        private Dictionary<string, WpfColor> shiftColors = new()
        {
            { "Morning", WpfColor.FromRgb(219, 234, 254) },    // Xanh nhạt
            { "Afternoon", WpfColor.FromRgb(254, 243, 199) },   // Vàng nhạt
            { "Evening", WpfColor.FromRgb(243, 232, 255) }      // Tím nhạt
        };

        public ucQuanLyCaLam()
        {
            InitializeComponent();
            SetCurrentWeek();
        }

        private void SetCurrentWeek()
        {
            // Tính ngày bắt đầu của tuần (luôn là Thứ 2)
            DateTime today = DateTime.Now;
            int daysToMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysToMonday < 0) daysToMonday += 7;
            currentWeekStart = today.AddDays(-daysToMonday);

            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void UpdateWeekDisplay()
        {
            DateTime weekEnd = currentWeekStart.AddDays(6);
            int weekNumber = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                currentWeekStart, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            txtTuanHienTai.Text = $"Tuần {weekNumber:D2} ({currentWeekStart:dd/MM} - {weekEnd:dd/MM/yyyy})";

            // Cập nhật mảng tiêu đề 7 ngày
            TextBlock[] dateBlocks = { txtDate0, txtDate1, txtDate2, txtDate3, txtDate4, txtDate5, txtDate6 };
            string[] dayNames = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };

            for (int i = 0; i < 7; i++)
            {
                dateBlocks[i].Text = $"{dayNames[i]}\n{currentWeekStart.AddDays(i):dd/MM}";
            }
        }

        private void LoadSchedule()
        {
            try
            {
                weekScheduleData.Clear();

                using (var db = new MilkTeaDBContext())
                {
                    DateTime weekEnd = currentWeekStart.AddDays(6);

                    // Sử dụng Join để lấy tên nhân viên dựa trên staffID trong lịch làm
                    var schedules = (from ws in db.WorkSchedules
                                     join s in db.Staffs on ws.staffID equals s.userID
                                     where ws.workDate >= currentWeekStart.Date && ws.workDate <= weekEnd.Date
                                     select new
                                     {
                                         ws.workDate,
                                         ws.shift,
                                         StaffName = s.Name
                                     }).ToList();

                    // Đưa vào Dictionary để dễ vẽ UI
                    foreach (var schedule in schedules)
                    {
                        string key = $"{schedule.workDate:yyyy-MM-dd}_{schedule.shift}";
                        if (!weekScheduleData.ContainsKey(key))
                        {
                            weekScheduleData[key] = new List<string>();
                        }
                        weekScheduleData[key].Add(schedule.StaffName);
                    }
                }

                RenderScheduleGrid();
            }
            catch (Exception) { }
        }

        private void RenderScheduleGrid()
        {
            // Xóa rác: Bỏ hết các hàng hiển thị cũ, chỉ giữ lại hàng 0 (Header các ngày)
            while (gridSchedule.RowDefinitions.Count > 1) gridSchedule.RowDefinitions.RemoveAt(gridSchedule.RowDefinitions.Count - 1);
            var childrenToRemove = gridSchedule.Children.OfType<UIElement>().Where(e => Grid.GetRow(e) >= 1).ToList();
            foreach (var child in childrenToRemove) gridSchedule.Children.Remove(child);

            int currentRow = 1;

            // Bắt đầu vẽ 3 Ca làm việc
            foreach (string shift in dbShifts)
            {
                // 1. Tạo thanh ngang tên Ca (VD: CA SÁNG)
                gridSchedule.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Border shiftHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(WpfColor.FromRgb(229, 231, 235)),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(2, 10, 2, 2),
                    Padding = new Thickness(5)
                };

                // ĐÃ SỬA LỖI CS0176: Chỉ định đích danh System.Windows.HorizontalAlignment.Center
                TextBlock shiftHeader = new TextBlock
                {
                    Text = shiftDisplayNames[shift],
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Colors.Black),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                shiftHeaderBorder.Child = shiftHeader;

                Grid.SetRow(shiftHeaderBorder, currentRow);
                Grid.SetColumn(shiftHeaderBorder, 0);
                Grid.SetColumnSpan(shiftHeaderBorder, 7);
                gridSchedule.Children.Add(shiftHeaderBorder);

                currentRow++;

                // 2. Tạo 7 ô cho 7 ngày của Ca đó
                gridSchedule.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                for (int dayIndex = 0; dayIndex < 7; dayIndex++)
                {
                    DateTime dayDate = currentWeekStart.AddDays(dayIndex);
                    string key = $"{dayDate:yyyy-MM-dd}_{shift}";

                    Border dayCell = new Border
                    {
                        Background = new SolidColorBrush(shiftColors[shift]),
                        CornerRadius = new CornerRadius(6),
                        BorderBrush = new SolidColorBrush(WpfColor.FromRgb(209, 213, 219)),
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(2),
                        Padding = new Thickness(8),
                        MinHeight = 100
                    };

                    StackPanel cellContent = new StackPanel { VerticalAlignment = VerticalAlignment.Top };

                    // Nếu có người làm ca này
                    if (weekScheduleData.ContainsKey(key) && weekScheduleData[key].Any())
                    {
                        foreach (var staffName in weekScheduleData[key])
                        {
                            Border nameBorder = new Border
                            {
                                Background = new SolidColorBrush(Colors.White),
                                CornerRadius = new CornerRadius(4),
                                Padding = new Thickness(5),
                                Margin = new Thickness(0, 0, 0, 5),
                                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(156, 163, 175)),
                                BorderThickness = new Thickness(0.5)
                            };
                            TextBlock staffText = new TextBlock
                            {
                                Text = $"• {staffName}",
                                FontSize = 13,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = new SolidColorBrush(WpfColor.FromRgb(31, 41, 55)),
                                TextWrapping = TextWrapping.Wrap
                            };
                            nameBorder.Child = staffText;
                            cellContent.Children.Add(nameBorder);
                        }
                    }
                    else
                    {
                        // Nếu ca trống
                        // ĐÃ SỬA LỖI CS0176: Chỉ định đích danh System.Windows.HorizontalAlignment.Center
                        cellContent.Children.Add(new TextBlock
                        {
                            Text = "Trống",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(WpfColor.FromRgb(156, 163, 175)),
                            FontStyle = FontStyles.Italic,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 20, 0, 0)
                        });
                    }

                    dayCell.Child = cellContent;
                    Grid.SetRow(dayCell, currentRow);
                    Grid.SetColumn(dayCell, dayIndex);
                    gridSchedule.Children.Add(dayCell);
                }
                currentRow++;
            }
        }

        private void btnTuanTruoc_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(-7);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void btnTuanSau_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(7);
            UpdateWeekDisplay();
            LoadSchedule();
        }
    }
}