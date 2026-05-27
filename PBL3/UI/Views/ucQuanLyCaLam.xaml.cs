using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using WpfColor = System.Windows.Media.Color;
using WpfButton = System.Windows.Controls.Button;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfMessageBox = System.Windows.MessageBox;
using PBL3.src.Application.Interface;
using PBL3.src.Infrastructure.Data;

namespace PBL3.UI.Views
{
    public partial class ucQuanLyCaLam : System.Windows.Controls.UserControl
    {
        private DateTime currentWeekStart;
        private Dictionary<string, List<(int scheduleId, string staffName)>> weekScheduleData = new();

        private List<string> dbShifts = new() { "Morning", "Afternoon", "Evening" };

        private Dictionary<string, string> shiftDisplayNames = new()
        {
            { "Morning", "CA SÁNG (08:00 - 13:00)" },
            { "Afternoon", "CA CHIỀU (13:00 - 18:00)" },
            { "Evening", "CA TỐI (18:00 - 22:00)" }
        };

        private Dictionary<string, WpfColor> shiftColors = new()
        {
            { "Morning", WpfColor.FromRgb(219, 234, 254) },   
            { "Afternoon", WpfColor.FromRgb(254, 243, 199) },   
            { "Evening", WpfColor.FromRgb(243, 232, 255) }
        };
        private readonly IStaffService _staffService;

        public ucQuanLyCaLam()
        {
            InitializeComponent();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();
            SetCurrentWeek();
        }

        private void SetCurrentWeek()
        {
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

                    var schedules = (from ws in db.WorkSchedules
                                     join s in db.Staffs on ws.staffID equals s.userID
                                     where ws.workDate >= currentWeekStart.Date && ws.workDate <= weekEnd.Date
                                     select new
                                     {
                                         ws.id,
                                         ws.workDate,
                                         ws.shift,
                                         StaffName = s.Name
                                     }).ToList();

                    foreach (var schedule in schedules)
                    {
                        string key = $"{schedule.workDate:yyyy-MM-dd}_{schedule.shift}";
                        if (!weekScheduleData.ContainsKey(key))
                        {
                            weekScheduleData[key] = new List<(int, string)>();
                        }
                        weekScheduleData[key].Add((schedule.id, schedule.StaffName));
                    }
                }

                RenderScheduleGrid();
            }
            catch (Exception) { }
        }

        private void RenderScheduleGrid()
        {
            while (gridSchedule.RowDefinitions.Count > 1) gridSchedule.RowDefinitions.RemoveAt(gridSchedule.RowDefinitions.Count - 1);
            var childrenToRemove = gridSchedule.Children.OfType<UIElement>().Where(e => Grid.GetRow(e) >= 1).ToList();
            foreach (var child in childrenToRemove) gridSchedule.Children.Remove(child);

            int currentRow = 1;

            foreach (string shift in dbShifts)
            {
                gridSchedule.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Border shiftHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(WpfColor.FromRgb(229, 231, 235)),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(2, 10, 2, 2),
                    Padding = new Thickness(5)
                };

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

                    if (weekScheduleData.ContainsKey(key) && weekScheduleData[key].Any())
                    {
                        foreach (var (scheduleId, staffName) in weekScheduleData[key])
                        {
                            StackPanel staffItemPanel = new StackPanel 
                            { 
                                Orientation = WpfOrientation.Horizontal,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 5)
                            };

                            Border nameBorder = new Border
                            {
                                Background = new SolidColorBrush(Colors.White),
                                CornerRadius = new CornerRadius(4),
                                Padding = new Thickness(5),
                                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(156, 163, 175)),
                                BorderThickness = new Thickness(0.5)
                            };
                            TextBlock staffText = new TextBlock
                            {
                                Text = $"• {staffName}",
                                FontSize = 13,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = new SolidColorBrush(WpfColor.FromRgb(31, 41, 55)),
                                TextWrapping = TextWrapping.Wrap,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            nameBorder.Child = staffText;
                            staffItemPanel.Children.Add(nameBorder);

                            // Nút xoá
                            WpfButton deleteBtn = new WpfButton
                            {
                                Content = "✕",
                                Tag = scheduleId,
                                Width = 25,
                                Height = 25,
                                Margin = new Thickness(5, 0, 0, 0),
                                Background = new SolidColorBrush(WpfColor.FromRgb(239, 68, 68)),
                                Foreground = new SolidColorBrush(Colors.White),
                                FontSize = 12,
                                FontWeight = FontWeights.Bold,
                                Cursor = System.Windows.Input.Cursors.Hand,
                                Padding = new Thickness(0),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            deleteBtn.Resources.Add("CornerRadius", new CornerRadius(4));
                            deleteBtn.Click += BtnDeleteSchedule_Click;
                            staffItemPanel.Children.Add(deleteBtn);

                            cellContent.Children.Add(staffItemPanel);
                        }
                    }
                    else
                    {
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

        private void BtnDeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            WpfButton btn = sender as WpfButton;
            if (btn != null && btn.Tag is int scheduleId)
            {
                var result = WpfMessageBox.Show(
                    "Bạn có chắc muốn xoá ca làm này?",
                    "Xác nhận xoá",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool isSuccess = _staffService.DeleteWorkSchedule(scheduleId);
                        if (isSuccess)
                        {
                            WpfMessageBox.Show(
                                "✓ Xoá ca làm thành công!",
                                "Thành công",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            LoadSchedule();
                        }
                        else
                        {
                            WpfMessageBox.Show(
                                "❌ Xoá ca làm thất bại!",
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        WpfMessageBox.Show(
                            $"❌ Lỗi: {ex.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}