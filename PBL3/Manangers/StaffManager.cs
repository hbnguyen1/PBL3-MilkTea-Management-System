using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq; // Bổ sung thư viện này để dùng các hàm Where, Any, FirstOrDefault
using System.Text;
using PBL3.Core;

namespace PBL3.Manangers
{
    internal class StaffManager
    {
        private DateTime GetStartOfWeek()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff).Date;
        }

        public List<WorkSchedule> GetMyWeeklySchedule(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime start = GetStartOfWeek();
                DateTime end = start.AddDays(7);

                return conn.WorkSchedules
                    .Where(s => s.staffID == staffID && s.workDate >= start && s.workDate < end)
                    .OrderBy(s => s.workDate)
                    .ToList();
            }
        }

        public string QuickRegisterShift(int staffID, DateTime date, string shift)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime start = GetStartOfWeek();
                DateTime end = start.AddDays(6);

                if (date.Date < start || date.Date > end)
                {
                    return "❌ Chỉ được đăng ký ca trong phạm vi tuần hiện tại!";
                }

                bool exists = conn.WorkSchedules.Any(s => s.staffID == staffID && s.workDate == date.Date && s.shift == shift);
                if (exists)
                {
                    return "⚠ Bạn đã đăng ký ca này rồi!";
                }

                conn.WorkSchedules.Add(new WorkSchedule
                {
                    staffID = staffID,
                    workDate = date.Date,
                    shift = shift
                });

                conn.SaveChanges();
                return "✔ Đăng ký ca làm thành công!";
            }
        }


        public (bool IsCheckedIn, string CurrentShift, int TimeRemainingMinutes, DateTime ShiftEnd) GetCheckOutStatus(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime today = DateTime.Today;
                DateTime now = DateTime.Now;

                var log = conn.WorkShiftLogs.FirstOrDefault(l =>
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
        }

        public bool ShouldShowCheckOutReminder(int staffID)
        {
            var (isCheckedIn, shift, timeRemaining, _) = GetCheckOutStatus(staffID);

            return isCheckedIn && timeRemaining >= 0 && timeRemaining <= 5;
        }

        public string GetCheckOutReminder(int staffID)
        {
            var (isCheckedIn, shift, timeRemaining, shiftEnd) = GetCheckOutStatus(staffID);

            if (!isCheckedIn)
            {
                return "";
            }

            if (timeRemaining > 5)
            {
                return ""; 
            }

            if (timeRemaining <= 0)
            {
                return $"⏰ CẢNH BÁO: Bạn đã quá giờ hết ca [{shift}]!\n" +
                       $"Vui lòng check-out ngay để tránh bị phạt thêm.";
            }

            return $"⏰ SẮP HẾT CA: Ca [{shift}] sẽ kết thúc trong {timeRemaining} phút!\n" +
                   $"Hãy chuẩn bị check-out lúc {shiftEnd:HH:mm}.";
        }

        public string ToggleShift(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime now = DateTime.Now;
                DateTime today = now.Date;

                string currentShift = GetCurrentShift(now);

                if (currentShift == "")
                {
                    currentShift = GetUpcomingShiftForCheckIn(now, today, conn, staffID);

                    if (currentShift == "")
                    {
                        return "❌ Hiện tại không nằm trong khung giờ của bất kỳ ca làm việc nào và không có ca sắp tới để check-in sớm!";
                    }
                }

                bool hasSchedule = conn.WorkSchedules
                    .Any(s => s.staffID == staffID
                           && s.workDate == today
                           && s.shift == currentShift);

                if (!hasSchedule)
                {
                    return $"❌ Bạn chưa đăng ký lịch làm việc cho ca [{currentShift}] ngày hôm nay!";
                }

                var log = conn.WorkShiftLogs.FirstOrDefault(l =>
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

                    conn.WorkShiftLogs.Add(log);
                    conn.SaveChanges();

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

                    conn.SaveChanges();

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
        }
        private string GetCurrentShift(DateTime now)
        {
            var t = now.TimeOfDay;

            if (t >= new TimeSpan(9, 0, 0) && t < new TimeSpan(13, 0, 0))
                return "Morning";

            if (t >= new TimeSpan(13, 0, 0) && t < new TimeSpan(18, 0, 0))
                return "Afternoon";

            if (t >= new TimeSpan(18, 0, 0) && t < new TimeSpan(22, 0, 0))
                return "Evening";

            return "";
        }

        private string GetUpcomingShiftForCheckIn(DateTime now, DateTime today, MilkTeaDBContext conn, int staffID)
        {
            var t = now.TimeOfDay;

            if (t >= new TimeSpan(7, 0, 0) && t < new TimeSpan(8, 0, 0))
            {
                return "Morning";
            }

            if (t >= new TimeSpan(12, 0, 0) && t < new TimeSpan(13, 0, 0))
            {
                return "Afternoon";
            }

            if (t >= new TimeSpan(17, 0, 0) && t < new TimeSpan(18, 0, 0))
            {
                return "Evening";
            }

            return "";
        }
        private DateTime GetShiftStart(string shift)
        {
            var today = DateTime.Today;

            return shift switch
            {
                "Morning" => today.AddHours(9),
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
                "Morning" => today.AddHours(13),
                "Afternoon" => today.AddHours(18),
                "Evening" => today.AddHours(22),
                _ => today
            };
        }
        public void ShowWeeklySchedule(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime start = GetStartOfWeek();

                Console.WriteLine("\n===== LỊCH LÀM VIỆC TUẦN =====");

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = start.AddDays(i);

                    var schedules = conn.WorkSchedules
                        .Where(s => s.staffID == staffID && s.workDate == day)
                        .Select(s => s.shift)
                        .ToList();

                    string morning = schedules.Contains("Morning") ? "✔" : "✘";
                    string afternoon = schedules.Contains("Afternoon") ? "✔" : "✘";
                    string evening = schedules.Contains("Evening") ? "✔" : "✘";

                    Console.WriteLine(
                        $"{day:dddd dd/MM} | M: {morning} | A: {afternoon} | E: {evening}"
                    );
                }
            }
        }
        public void RegisterWeeklySchedule(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime start = GetStartOfWeek();
                DateTime end = start.AddDays(6);

                while (true)
                {
                    ShowWeeklySchedule(staffID);

                    Console.WriteLine("\nNhập lịch (vd: 2/4 - AE, 3/4 A E) hoặc 0 để thoát:");
                    Console.WriteLine("M = Morning | A = Afternoon | E = Evening");

                    string input = Console.ReadLine();
                    if (input == "0") break;

                    var entries = input.Split(',');

                    foreach (var entry in entries)
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
                                    Console.WriteLine($"Sai format: {entry}");
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
                                    Console.WriteLine($"Sai format: {entry}");
                                    continue;
                                }

                                datePart = parts[0].Trim();
                                shiftPart = string.Join("", parts.Skip(1)).ToUpper();
                            }

                            var dateSplit = datePart.Split('/');
                            if (dateSplit.Length != 2)
                            {
                                Console.WriteLine($"Sai ngày: {datePart}");
                                continue;
                            }

                            int day = int.Parse(dateSplit[0]);
                            int month = int.Parse(dateSplit[1]);

                            DateTime chosenDay = new DateTime(DateTime.Now.Year, month, day);

                            if (chosenDay < start || chosenDay > end)
                            {
                                Console.WriteLine($"Ngày {datePart} không thuộc tuần này!");
                                continue;
                            }

                            var shiftTokens = shiftPart
                                .Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                            List<string> shifts = new List<string>();

                            foreach (var token in shiftTokens)
                            {
                                foreach (char c in token)
                                {
                                    switch (c)
                                    {
                                        case 'M':
                                            shifts.Add("Morning");
                                            break;
                                        case 'A':
                                            shifts.Add("Afternoon");
                                            break;
                                        case 'E':
                                            shifts.Add("Evening");
                                            break;
                                        default:
                                            Console.WriteLine($"Ca không hợp lệ: {c}");
                                            break;
                                    }
                                }
                            }

                            if (shifts.Count == 0)
                            {
                                Console.WriteLine($"Không có ca hợp lệ: {entry}");
                                continue;
                            }

                            foreach (var shift in shifts)
                            {
                                bool exists = conn.WorkSchedules.Any(s =>
                                    s.staffID == staffID &&
                                    s.workDate == chosenDay &&
                                    s.shift == shift);

                                if (exists)
                                {
                                    Console.WriteLine($"Đã tồn tại: {datePart} - {shift}");
                                    continue;
                                }

                                WorkSchedule ws = new WorkSchedule
                                {
                                    staffID = staffID,
                                    workDate = chosenDay,
                                    shift = shift
                                };

                                conn.WorkSchedules.Add(ws);

                                Console.WriteLine($"✔ Đã thêm: {datePart} - {shift}");
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"Sai format: {entry}");
                        }
                    }

                    conn.SaveChanges();
                    Console.WriteLine("\n✔ Lưu thành công!\n");
                }
            }
        }
        public double CalculateSalary(int staffID, int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var staff = conn.Staffs.Find(staffID);

                if (staff == null)
                {
                    return 0; 
                }

                var logs = conn.WorkShiftLogs
                    .Where(l => l.staffID == staffID
                             && l.checkOut != null
                             && l.workDate.Month == month
                             && l.workDate.Year == year)
                    .ToList();

                double totalHours = logs.Sum(l => l.totalHours);
                int totalPenalty = logs.Sum(l => l.penalty);

                double salary = (totalHours * staff.salaryPerHour) - totalPenalty;

                if (salary < 0)
                {
                    salary = 0;
                }

                return salary; 
            }
        }
        public string SaveSalary(int staffID, int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var exist = conn.SalarySummaries
                    .FirstOrDefault(x => x.staffID == staffID && x.month == month && x.year == year);

                if (exist != null)
                {
                    return "⚠ Nhân viên này đã được chốt lương trong tháng này rồi!";
                }

                var logs = conn.WorkShiftLogs
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

                conn.SalarySummaries.Add(new SalarySummary
                {
                    staffID = staffID,
                    month = month,
                    year = year,
                    totalHours = totalHours,
                    penalty = totalPenalty,
                    totalSalary = salary
                });

                conn.SaveChanges();
                return "✔ Chốt lương thành công!";
            }
        }

    }
}