using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
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
        public void ShowWeeklySchedule(int staffID, DateTime start, DateTime end)
        {
            using (var conn = new MilkTeaDBContext())
            {
                Console.WriteLine("\n===== LỊCH LÀM VIỆC TUẦN =====");

                for (DateTime day = start; day <= end; day = day.AddDays(1))
                {
                    var schedules = conn.WorkSchedules
                        .Where(s => s.staffID == staffID && s.workDate.Date == day.Date)
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
                Console.WriteLine("Chọn tuần:");
                Console.WriteLine("0. Tuần này");
                Console.WriteLine("1. Tuần sau");
                Console.Write("Nhập (0/1/...): ");

                int offset = 0;
                int.TryParse(Console.ReadLine(), out offset);

                DateTime start = GetStartOfWeek().AddDays(offset * 7);
                DateTime end = start.AddDays(6);

                while (true)
                {
                    int addedCount = 0; 

                    ShowWeeklySchedule(staffID, start, end);

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
                                    Console.WriteLine($"❌ Sai format: {entry}");
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
                                    Console.WriteLine($"❌ Sai format: {entry}");
                                    continue;
                                }

                                datePart = parts[0].Trim();
                                shiftPart = string.Join("", parts.Skip(1)).ToUpper();
                            }

                            var dateSplit = datePart.Split('/');
                            if (dateSplit.Length != 2)
                            {
                                Console.WriteLine($"❌ Sai ngày: {datePart}");
                                continue;
                            }

                            int day = int.Parse(dateSplit[0]);
                            int month = int.Parse(dateSplit[1]);

                            DateTime chosenDay = new DateTime(DateTime.Now.Year, month, day);

                            if (chosenDay < start || chosenDay > end)
                            {
                                Console.WriteLine($"❌ Ngày {datePart} không thuộc tuần đã chọn!");
                                continue;
                            }

                            List<string> shifts = new List<string>();

                            foreach (char c in shiftPart)
                            {
                                switch (c)
                                {
                                    case 'M': shifts.Add("Morning"); break;
                                    case 'A': shifts.Add("Afternoon"); break;
                                    case 'E': shifts.Add("Evening"); break;
                                    default:
                                        Console.WriteLine($"❌ Ca không hợp lệ: {c}");
                                        break;
                                }
                            }

                            if (shifts.Count == 0)
                            {
                                Console.WriteLine($"❌ Không có ca hợp lệ: {entry}");
                                continue;
                            }

                            foreach (var shift in shifts)
                            {
                                bool exists = conn.WorkSchedules.Any(s =>
                                    s.staffID == staffID &&
                                    s.workDate.Date == chosenDay.Date &&
                                    s.shift == shift);

                                if (exists)
                                {
                                    Console.WriteLine($"⚠ Bạn đã đăng ký: {datePart} - {shift}");
                                    continue;
                                }

                                int count = conn.WorkSchedules.Count(s =>
                                    s.workDate.Date == chosenDay.Date &&
                                    s.shift == shift);

                                if (count >= 5)
                                {
                                    Console.WriteLine($"❌ Ca {shift} ngày {datePart} đã đủ 5 người!");
                                    continue;
                                }

                                WorkSchedule ws = new WorkSchedule
                                {
                                    staffID = staffID,
                                    workDate = chosenDay,
                                    shift = shift
                                };

                                conn.WorkSchedules.Add(ws);
                                addedCount++;

                                Console.WriteLine($"✔ Đã thêm: {datePart} - {shift} (còn {5 - count - 1} chỗ)");
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"❌ Sai format: {entry}");
                        }
                    }

                    if (addedCount > 0)
                    {
                        conn.SaveChanges();
                        Console.WriteLine($"\n✔ Lưu thành công {addedCount} ca!\n");
                    }
                    else
                    {
                        Console.WriteLine("\n⚠ Không có ca nào được thêm!\n");
                    }
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

        public void StaffCreateOrderForCustomer()
        {
            UserService userService = new UserService();
            OrderManager orderManager = new OrderManager();
            Console.WriteLine("Bạn đã có tài khoản chưa: ");
            Console.WriteLine("1. Đã có tài khoản: ");
            Console.WriteLine("2. Tạo tài khoản mới: ");
            Console.WriteLine("3. Chị không có nhu cầu em ơi");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Nhập số điện thoại khách hàng: ");
                    string phone = Console.ReadLine();
                    Users customer = userService.GetUserByPhone(phone);
                    orderManager.Order(customer);
                    break;
                case "2":
                    string name = Console.ReadLine();
                    string phoneNumber = Console.ReadLine();
                    string password = Console.ReadLine();
                    CustomerManagers.Register(name, phoneNumber, password);
                    Users customerr = userService.GetUserByPhone(phoneNumber);
                    orderManager.Order(customerr);
                    break;
                case "3":
                    string phonenumber = "0000000000";
                    Users guest = userService.GetUserByPhone(phonenumber);
                    orderManager.Order(guest);
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }
}
