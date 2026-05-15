using PBL3.Models;
using System;
using System.Collections.Generic;

namespace PBL3.Interface
{
    public interface IStaffService
    {
        List<WorkSchedule> GetMyWeeklySchedule(int staffID);
        string QuickRegisterShift(int staffID, DateTime date, string shift);
        (bool IsCheckedIn, string CurrentShift, int TimeRemainingMinutes, DateTime ShiftEnd) GetCheckOutStatus(int staffID);
        bool ShouldShowCheckOutReminder(int staffID);
        string GetCheckOutReminder(int staffID);
        string ToggleShift(int staffID);
        void ShowWeeklySchedule(int staffID);
        void RegisterWeeklySchedule(int staffID);
        double CalculateSalary(int staffID, int month, int year);
        string SaveSalary(int staffID, int month, int year);
        List<Staff> GetAllStaffs();
        List<WorkShiftLog> GetShiftLogs(int staffID, int month, int year);
        bool IsSalarySaved(int staffID, int month, int year);
    }
}