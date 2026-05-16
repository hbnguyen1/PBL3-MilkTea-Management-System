using PBL3.Data;
using BCrypt.Net;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBL3.Core;

namespace PBL3.Service 
{
    internal class StaffService : IStaffService
    {
        private const int MAX_STAFF_PER_SHIFT = 2;

        private readonly MilkTeaDBContext _conn;

        private static readonly object _shiftLock = new object();
        public StaffService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        private DateTime GetStartOfWeek()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff).Date;
        }

        private int GetRegisteredStaffCount(DateTime workDate, string shift)
        {
            return _conn.WorkSchedules
                .Where(s => s.workDate == workDate && s.shift == shift)
                .Select(s => s.staffID)
                .Distinct()
                .Count();
        }

        private bool CanRegisterShift(DateTime workDate, string shift)
        {
            int count = GetRegisteredStaffCount(workDate, shift);
            return count < MAX_STAFF_PER_SHIFT;
        }

        public int GetRegisteredStaffCountForShift(DateTime workDate, string shift)
        {
            return GetRegisteredStaffCount(workDate, shift);
        }

        public int GetRemainingSpots(DateTime workDate, string shift)
        {
            int registered = GetRegisteredStaffCount(workDate, shift);
            return Math.Max(0, MAX_STAFF_PER_SHIFT - registered);
        }

        public List<WorkSchedule> GetMyWeeklySchedule(int staffID)
        {
            DateTime start = GetStartOfWeek();
            DateTime end = start.AddDays(7);

            return _conn.WorkSchedules
                .Where(s => s.staffID == staffID && s.workDate >= start && s.workDate < end)
                .OrderBy(s => s.workDate)
                .ToList();
        }

        public string QuickRegisterShift(int staffID, DateTime date, string shift)
        {
            lock (_shiftLock)
            {
                DateTime start = GetStartOfWeek();
                DateTime end = start.AddDays(6);

                if (date.Date < start || date.Date > end)
                {
                    return "❌ Chỉ được đăng ký ca trong phạm vi tuần hiện tại!";
                }

                bool exists = _conn.WorkSchedules.Any(s => s.staffID == staffID && s.workDate == date.Date && s.shift == shift);
                if (exists)
                {
                    return "⚠ Bạn đã đăng ký ca này rồi!";
                }

                int registeredCount = _conn.WorkSchedules
                    .Where(s => s.workDate == date.Date && s.shift == shift)
                    .Select(s => s.staffID)
                    .Distinct()
                    .Count();

                if (registeredCount >= MAX_STAFF_PER_SHIFT)
                {
                    return $"❌ Ca này đã đủ {MAX_STAFF_PER_SHIFT} nhân viên, không thể đăng ký thêm!";
                }

                _conn.WorkSchedules.Add(new WorkSchedule
                {
                    staffID = staffID,
                    workDate = date.Date,
                    shift = shift
                });

                _conn.SaveChanges();
                return "✔ Đăng ký ca làm thành công!";
            }
        }

        public (bool IsCheckedIn, string CurrentShift, int TimeRemainingMinutes, DateTime ShiftEnd) GetCheckOutStatus(int staffID)
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;

            var log = _conn.WorkShiftLogs.FirstOrDefault(l =>
                l.staffID == staffID &&
                l.workDate == today &&
                l.checkIn != null &&
                l.checkOut == null);

            if (log == null)
            {
                return (false, "", 0, DateTime.MinValue);
            }

            DateTime shiftEnd = GetShiftEnd(log.shift);
            int timeRemaining = (int)(shiftEnd - now).TotalMinutes;

            return (true, log.shift, timeRemaining, shiftEnd);
        }

        public bool ShouldShowCheckOutReminder(int staffID)
        {
            var (isCheckedIn, shift, timeRemaining, _) = GetCheckOutStatus(staffID);
            return isCheckedIn && timeRemaining >= 0 && timeRemaining <= 5;
        }

        public string GetCheckOutReminder(int staffID)
        {
            var (isCheckedIn, shift, timeRemaining, shiftEnd) = GetCheckOutStatus(staffID);

            if (!isCheckedIn) return "";
            if (timeRemaining > 5) return "";

            if (timeRemaining <= 0)
            {
                return $"⏰ CẢNH BÁO: Bạn đã quá giờ hết ca [{shift}]!\nVui lòng check-out ngay để tránh bị phạt thêm.";
            }

            return $"⏰ SẮP HẾT CA: Ca [{shift}] sẽ kết thúc trong {timeRemaining} phút!\nHãy chuẩn bị check-out lúc {shiftEnd:HH:mm}.";
        }

        public string ToggleShift(int staffID)
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            string currentShift = GetCurrentShift(now);

            if (currentShift == "")
            {
                currentShift = GetUpcomingShiftForCheckIn(now, today, _conn, staffID);

                if (currentShift == "")
                {
                    return "❌ Hiện tại không nằm trong khung giờ của bất kỳ ca làm việc nào và không có ca sắp tới để check-in sớm!";
                }
            }

            bool hasSchedule = _conn.WorkSchedules
                .Any(s => s.staffID == staffID
                       && s.workDate == today
                       && s.shift == currentShift);

            if (!hasSchedule)
            {
                return $"❌ Bạn chưa đăng ký lịch làm việc cho ca [{currentShift}] ngày hôm nay!";
            }

            var log = _conn.WorkShiftLogs.FirstOrDefault(l =>
                l.staffID == staffID &&
                l.workDate == today &&
                l.shift == currentShift);

            if (log == null)
            {
                DateTime start = GetShiftStart(currentShift);
                int lateMinutes = (int)(now - start).TotalMinutes;
                int penalty = 0;

                if (lateMinutes > 10)
                {
                    penalty = (lateMinutes / 10) * 5000;
                }

                log = new WorkShiftLog
                {
                    staffID = staffID,
                    workDate = today,
                    shift = currentShift,
                    checkIn = now,
                    penalty = penalty
                };

                _conn.WorkShiftLogs.Add(log);
                _conn.SaveChanges();

                string resultMsg = $"✔ Check-in ca [{currentShift}] thành công lúc {now:HH:mm}!";
                if (penalty > 0)
                {
                    resultMsg += $"\n⚠ Lưu ý: Bạn đi trễ {lateMinutes} phút. Hệ thống ghi nhận mức phạt {penalty:N0}đ.";
                }
                else if (lateMinutes < 0)
                {
                    resultMsg += $"\n✨ Bạn check-in sớm {-lateMinutes} phút. Tuyệt vời!";
                }
                return resultMsg;
            }
            else if (log.checkOut == null)
            {
                DateTime start = GetShiftStart(currentShift);
                DateTime end = GetShiftEnd(currentShift);

                int earlyMinutes = (int)(end - now).TotalMinutes;
                int extraPenalty = 0;

                if (earlyMinutes > 15)
                {
                    extraPenalty = (earlyMinutes / 10) * 5000;
                    log.penalty += extraPenalty;
                }

                log.checkOut = now;

                DateTime actualEnd = now > end ? end : now;
                TimeSpan workDuration = actualEnd - start;
                log.totalHours = workDuration.TotalHours;

                _conn.SaveChanges();

                string resultMsg = $"✔ Check-out ca [{currentShift}] thành công lúc {now:HH:mm}!";
                resultMsg += $"\n⏳ Tổng thời gian làm việc: {log.totalHours:F2} giờ.";

                if (extraPenalty > 0)
                {
                    resultMsg += $"\n⚠ Lưu ý: Bạn về sớm {earlyMinutes} phút. Hệ thống ghi nhận thêm mức phạt {extraPenalty:N0}đ.";
                }
                return resultMsg;
            }
            else
            {
                return $"✔ Bạn đã hoàn thành xuất sắc ca [{currentShift}] rồi. Hãy nghỉ ngơi nhé!";
            }
        }

        private string GetCurrentShift(DateTime now)
        {
            var t = now.TimeOfDay;
            if (t >= new TimeSpan(8, 0, 0) && t < new TimeSpan(12, 0, 0)) return "Morning";
            if (t >= new TimeSpan(13, 0, 0) && t < new TimeSpan(18, 0, 0)) return "Afternoon";
            if (t >= new TimeSpan(18, 0, 0) && t < new TimeSpan(22, 0, 0)) return "Evening";
            return "";
        }

        private string GetUpcomingShiftForCheckIn(DateTime now, DateTime today, MilkTeaDBContext conn, int staffID)
        {
            var t = now.TimeOfDay;
            if (t >= new TimeSpan(7, 0, 0) && t < new TimeSpan(8, 0, 0)) return "Morning";
            if (t >= new TimeSpan(12, 0, 0) && t < new TimeSpan(13, 0, 0)) return "Afternoon";
            if (t >= new TimeSpan(17, 0, 0) && t < new TimeSpan(18, 0, 0)) return "Evening";
            return "";
        }

        private DateTime GetShiftStart(string shift)
        {
            var today = DateTime.Today;
            return shift switch
            {
                "Morning" => today.AddHours(8),
                "Afternoon" => today.AddHours(13),
                "Evening" => today.AddHours(18),
                _ => today
            };
        }

        private DateTime GetShiftEnd(string shift)
        {
            var today = DateTime.Today;
            return shift switch
            {
                "Morning" => today.AddHours(12),
                "Afternoon" => today.AddHours(18),
                "Evening" => today.AddHours(22),
                _ => today
            };
        }

        public void ShowWeeklySchedule(int staffID)
        {
            DateTime start = GetStartOfWeek();
            Console.WriteLine("\n===== LỊCH LÀM VIỆC TUẦN =====");

            for (int i = 0; i < 7; i++)
            {
                DateTime day = start.AddDays(i);
                var schedules = _conn.WorkSchedules
                    .Where(s => s.staffID == staffID && s.workDate == day)
                    .Select(s => s.shift)
                    .ToList();

                string morning = schedules.Contains("Morning") ? "✔" : "✘";
                string afternoon = schedules.Contains("Afternoon") ? "✔" : "✘";
                string evening = schedules.Contains("Evening") ? "✔" : "✘";

                Console.WriteLine($"{day:dddd dd/MM} | M: {morning} | A: {afternoon} | E: {evening}");
            }
        }

        public List<string> RegisterWeeklySchedule(int staffID, List<string> scheduleEntries)
        {
            var messages = new List<string>();
            DateTime start = GetStartOfWeek();
            DateTime end = start.AddDays(6);

            lock (_shiftLock) // ✅ Đồng bộ khóa luồng khi đăng ký lịch tuần loạt lớn
            {
                foreach (var entry in scheduleEntries)
                {
                    try
                    {
                        string datePart = "";
                        string shiftPart = "";

                        if (entry.Contains("-"))
                        {
                            var parts = entry.Split('-');
                            if (parts.Length != 2)
                            {
                                messages.Add($"❌ Sai format: {entry}");
                                continue;
                            }
                            datePart = parts[0].Trim();
                            shiftPart = parts[1].Trim().ToUpper();
                        }
                        else
                        {
                            var parts = entry.Trim().Split(' ');
                            if (parts.Length < 2)
                            {
                                messages.Add($"❌ Sai format: {entry}");
                                continue;
                            }
                            datePart = parts[0].Trim();
                            shiftPart = string.Join("", parts.Skip(1)).ToUpper();
                        }

                        var dateSplit = datePart.Split('/');
                        if (dateSplit.Length != 2)
                        {
                            messages.Add($"❌ Sai ngày: {datePart}");
                            continue;
                        }

                        int day = int.Parse(dateSplit[0]);
                        int month = int.Parse(dateSplit[1]);
                        DateTime chosenDay = new DateTime(DateTime.Now.Year, month, day);

                        if (chosenDay < start || chosenDay > end)
                        {
                            messages.Add($"❌ Ngày {datePart} không thuộc tuần này!");
                            continue;
                        }

                        var shiftTokens = shiftPart.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> shifts = new List<string>();

                        foreach (var token in shiftTokens)
                        {
                            foreach (char c in token)
                            {
                                switch (c)
                                {
                                    case 'M': shifts.Add("Morning"); break;
                                    case 'A': shifts.Add("Afternoon"); break;
                                    case 'E': shifts.Add("Evening"); break;
                                }
                            }
                        }

                        if (shifts.Count == 0)
                        {
                            messages.Add($"❌ Không có ca hợp lệ: {entry}");
                            continue;
                        }

                        foreach (var shift in shifts)
                        {
                            bool exists = _conn.WorkSchedules.Any(s =>
                                s.staffID == staffID &&
                                s.workDate == chosenDay &&
                                s.shift == shift);

                            if (exists)
                            {
                                messages.Add($"⚠ Đã tồn tại: {datePart} - {shift}");
                                continue;
                            }

                            int registeredCount = _conn.WorkSchedules
                                .Where(s => s.workDate == chosenDay && s.shift == shift)
                                .Select(s => s.staffID)
                                .Distinct()
                                .Count();

                            if (registeredCount >= MAX_STAFF_PER_SHIFT)
                            {
                                messages.Add($"❌ Ca {shift} ngày {datePart} đã đủ {MAX_STAFF_PER_SHIFT} nhân viên!");
                                continue;
                            }

                            _conn.WorkSchedules.Add(new WorkSchedule
                            {
                                staffID = staffID,
                                workDate = chosenDay,
                                shift = shift
                            });
                            messages.Add($"✔ Đã thêm: {datePart} - {shift}");
                        }
                    }
                    catch
                    {
                        messages.Add($"❌ Sai format: {entry}");
                    }
                }

                _conn.SaveChanges();
                messages.Add("✔ Lưu thành công!");
            }

            return messages;
        }

        public void RegisterWeeklyScheduleConsole(int staffID)
        {
            while (true)
            {
                ShowWeeklySchedule(staffID);
                Console.WriteLine("\nNhập lịch (vd: 2/4 - AE, 3/4 A E) hoặc 0 để thoát:");
                Console.WriteLine("M = Morning | A = Afternoon | E = Evening");

                string input = Console.ReadLine();
                if (input == "0") break;

                var entries = input.Split(',').ToList();
                var messages = RegisterWeeklySchedule(staffID, entries);

                foreach (var msg in messages) Console.WriteLine(msg);
            }
        }

        public double CalculateSalary(int staffID, int month, int year)
        {
            var staff = _conn.Staffs.Find(staffID);
            if (staff == null) return 0;

            var logs = _conn.WorkShiftLogs
                .Where(l => l.staffID == staffID
                         && l.checkOut != null
                         && l.workDate.Month == month
                         && l.workDate.Year == year)
                .ToList();

            double totalHours = logs.Sum(l => l.totalHours);
            int totalPenalty = logs.Sum(l => l.penalty);

            double salary = (totalHours * staff.salaryPerHour) - totalPenalty;
            return salary < 0 ? 0 : salary;
        }

        public string SaveSalary(int staffID, int month, int year)
        {
            var exist = _conn.SalarySummaries
                .FirstOrDefault(x => x.staffID == staffID && x.month == month && x.year == year);

            if (exist != null)
            {
                return "⚠ Nhân viên này đã được chốt lương trong tháng này rồi!";
            }

            var logs = _conn.WorkShiftLogs
                .Where(l => l.staffID == staffID
                         && l.checkOut != null
                         && l.workDate.Month == month
                         && l.workDate.Year == year)
                .ToList();

            if (logs.Count == 0)
            {
                return "❌ Nhân viên không có ca làm việc nào hoàn thành trong tháng này!";
            }

            double totalHours = logs.Sum(l => l.totalHours);
            int totalPenalty = logs.Sum(l => l.penalty);
            double salary = CalculateSalary(staffID, month, year);

            _conn.SalarySummaries.Add(new SalarySummary
            {
                staffID = staffID,
                month = month,
                year = year,
                totalHours = totalHours,
                penalty = totalPenalty,
                totalSalary = salary
            });

            _conn.SaveChanges();
            return "✔ Chốt lương thành công!";
        }

        public bool AddNewStaff(string name, string phoneNumber, string password, string role, double salaryPerHour)
        {
            var existingUser = _conn.Users.SingleOrDefault(u => u.Phone == phoneNumber);
            if (existingUser != null) return false;

            var newStaff = new Staff
            {
                Name = name,
                Phone = phoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                salaryPerHour = (int)salaryPerHour
            };

            _conn.Staffs.Add(newStaff);
            _conn.SaveChanges();

            return true;
        }
        public List<Staff> GetAllStaffs()
        {
            return _conn.Staffs.ToList();
        }

        public List<WorkShiftLog> GetShiftLogs(int staffID, int month, int year)
        {
            return _conn.WorkShiftLogs
                        .Where(l => l.staffID == staffID && l.workDate.Month == month && l.workDate.Year == year)
                        .OrderBy(l => l.workDate)
                        .ToList();
        }

        public bool IsSalarySaved(int staffID, int month, int year)
        {
            return _conn.SalarySummaries.Any(s => s.staffID == staffID && s.month == month && s.year == year);
        }
    }
}