using System;
using System.Collections.Generic;
using PBL3.Models;

namespace PBL3.Interface
{
    public interface IStaffService
    {
        int GetRegisteredStaffCountForShift(DateTime workDate, string shift);

        int GetRemainingSpots(DateTime workDate, string shift);

        List<WorkSchedule> GetMyWeeklySchedule(int staffID);

        string QuickRegisterShift(int staffID, DateTime date, string shift);
        (bool IsCheckedIn, string CurrentShift, int TimeRemainingMinutes, DateTime ShiftEnd) GetCheckOutStatus(int staffID);

        bool ShouldShowCheckOutReminder(int staffID);

        string GetCheckOutReminder(int staffID);

        string ToggleShift(int staffID);

        void ShowWeeklySchedule(int staffID);

        List<string> RegisterWeeklySchedule(int staffID, List<string> scheduleEntries);

        void RegisterWeeklyScheduleConsole(int staffID);
        double CalculateSalary(int staffID, int month, int year);

        string SaveSalary(int staffID, int month, int year);

        bool AddNewStaff(string name, string phoneNumber, string password, string role, double salaryPerHour);
        public List<Staff> GetAllStaffs();
        public bool IsSalarySaved(int staffID, int month, int year);
        public List<WorkShiftLog> GetShiftLogs(int staffID, int month, int year);
    }
}