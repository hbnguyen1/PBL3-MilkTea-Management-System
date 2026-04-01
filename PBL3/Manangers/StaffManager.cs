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
                var current = conn.WorkShiftLogs
                    .FirstOrDefault(s => s.staffID == staffID && s.checkOutTime == null);

                if (current == null)
                {
                    // CHECK-IN
                    WorkShiftLog log = new WorkShiftLog
                    {
                        staffID = staffID,
                        checkInTime = DateTime.Now
                    };

                    conn.WorkShiftLogs.Add(log);
                    conn.SaveChanges();

                    Console.WriteLine("Check-in thành công!");
                }
                else
                {
                    // CHECK-OUT
                    current.checkOutTime = DateTime.Now;

                    TimeSpan time = current.checkOutTime.Value - current.checkInTime;
                    current.totalHours = time.TotalHours;

                    conn.SaveChanges();

                    Console.WriteLine($"Check-out thành công! Bạn đã làm {current.totalHours:F2} giờ");
                }
            }
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

                            // ===== 2. PARSE INPUT LINH HOẠT =====
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
                                // gộp lại thành AE
                            }

                            // ===== 3. XỬ LÝ NGÀY =====
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

                            // ===== 4. XỬ LÝ NHIỀU CA (CHẤP NHẬN A E, A,E, AE) =====
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

                            // ===== 5. LƯU DB =====
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
    }
}
