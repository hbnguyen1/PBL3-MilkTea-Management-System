using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
        public void ToggleShift(int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime now = DateTime.Now;
                DateTime today = now.Date;

                // 1. xác định ca hiện tại
                string currentShift = GetCurrentShift(now);

                if (currentShift == "")
                {
                    Console.WriteLine("❌ Không nằm trong giờ ca!");
                    return;
                }

                // 2. kiểm tra có đăng ký ca không
                bool hasSchedule = conn.WorkSchedules
                    .Any(s => s.staffID == staffID
                           && s.workDate == today
                           && s.shift == currentShift);

                if (!hasSchedule)
                {
                    Console.WriteLine($"❌ Bạn không có ca {currentShift} hôm nay!");
                    return;
                }

                // 3. tìm log
                var log = conn.WorkShiftLogs.FirstOrDefault(l =>
                    l.staffID == staffID &&
                    l.workDate == today &&
                    l.shift == currentShift);

                if (log == null)
                {
                    // ===== CHECK-IN =====
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

                    Console.WriteLine($"✔ Check-in {currentShift}");

                    if (penalty > 0)
                        Console.WriteLine($"⚠ Trễ {lateMinutes} phút → phạt {penalty}đ");
                }
                else if (log.checkOut == null)
                {
                    // ===== CHECK-OUT =====
                    DateTime end = GetShiftEnd(currentShift);

                    int earlyMinutes = (int)(end - now).TotalMinutes;

                    if (earlyMinutes > 15)
                    {
                        int penalty = (earlyMinutes / 10) * 5000;
                        log.penalty += penalty;

                        Console.WriteLine($"⚠ Về sớm {earlyMinutes} phút → phạt {penalty}đ");
                    }

                    log.checkOut = now;

                    // tính giờ làm
                    if (log.checkIn != null)
                    {
                        TimeSpan work = log.checkOut.Value - log.checkIn.Value;
                        log.totalHours = work.TotalHours;
                    }

                    conn.SaveChanges();

                    Console.WriteLine($"✔ Check-out {currentShift} - Làm {log.totalHours:F2} giờ");
                }
                else
                {
                    Console.WriteLine("❌ Bạn đã hoàn thành ca này rồi!");
                }
            }
        }
        private string GetCurrentShift(DateTime now)
        {
            var t = now.TimeOfDay;

            if (t >= new TimeSpan(8, 0, 0) && t < new TimeSpan(13, 0, 0))
                return "Morning";

            if (t >= new TimeSpan(13, 0, 0) && t < new TimeSpan(18, 0, 0))
                return "Afternoon";

            if (t >= new TimeSpan(18, 0, 0) && t < new TimeSpan(22, 0, 0))
                return "Evening";

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
                    // ===== 1. HIỂN THỊ BẢNG =====
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
                    Console.WriteLine("Không tìm thấy nhân viên!");
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

                double salary = totalHours * staff.salaryPerHour - totalPenalty;

                Console.WriteLine($"===== LƯƠNG THÁNG {month}/{year} =====");
                Console.WriteLine($"Tổng giờ: {totalHours:F2}");
                Console.WriteLine($"Phạt: {totalPenalty}");
                Console.WriteLine($"Lương: {salary}");

                return salary;
            }
        }
        public void SaveSalary(int staffID, int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var exist = conn.SalarySummaries
                    .FirstOrDefault(x => x.staffID == staffID && x.month == month && x.year == year);

                if (exist != null)
                {
                    Console.WriteLine("Đã có lương tháng này rồi!");
                    return;
                }

                var logs = conn.WorkShiftLogs
                    .Where(l => l.staffID == staffID
                             && l.checkOut != null
                             && l.workDate.Month == month
                             && l.workDate.Year == year)
                    .ToList();

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

                Console.WriteLine("Đã lưu lương!");
            }
        }
    }
}
