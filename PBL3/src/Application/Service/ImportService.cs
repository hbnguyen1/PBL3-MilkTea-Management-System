using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.src.Domain.Models;
using PBL3.src.Application.Interface;
using PBL3.src.Infrastructure.Data;
using PBL3.src.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace PBL3.src.Application.Service
{
    internal class ImportService : IImportService
    {
        private readonly MilkTeaDBContext _conn;
        public ImportService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }
        public bool CreateImport(int staffId, List<ImportDetail> details)
        {
                using (var transaction = _conn.Database.BeginTransaction())
                {
                    try
                    {
                        int totalCost = 0;

                        // Tính tổng tiền
                        foreach (var d in details)
                        {
                            totalCost += d.quantityAdded * d.importPrice;
                        }

                        //Tạo phiếu nhập
                        ImportNote note = new ImportNote
                        {
                            importDate = DateTime.Now,
                            staffID = staffId,
                            totalCost = totalCost
                        };

                        _conn.ImportNotes.Add(note);
                        _conn.SaveChanges();

                        Logger.Info($"Tạo phiếu nhập ID {note.importID} bởi staff {staffId}");

                        foreach (var d in details)
                        {
                            d.importId = note.importID;

                            _conn.ImportDetails.Add(d);

                            var ig = _conn.Ingredients.Find(d.igId);

                            if (ig != null)
                            {
                                ig.igCount += d.quantityAdded;

                                Logger.Info($"Nhập {d.quantityAdded} {ig.igName} (giá {d.importPrice})");
                            }
                        }

                        _conn.SaveChanges();
                        transaction.Commit();

                        Logger.Info($"Hoàn tất nhập kho phiếu {note.importID}, tổng tiền {totalCost}");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Logger.Error("Lỗi nhập kho: " + ex.Message);

                        Console.WriteLine("LỖI CHI TIẾT: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("SQL Server báo: " + ex.InnerException.Message);
                        }
                        System.Windows.MessageBox.Show("Lỗi SQL: " + ex.InnerException?.Message ?? ex.Message);
                        return false;
                    }
                }

        }

        public List<ImportNote> GetAllImports()
        {
            try
            {
                return _conn.ImportNotes
                    .Include(x => x.ImportDetails)
                    .ThenInclude(x => x.Ingredient)
                    .OrderByDescending(x => x.importDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi lấy danh sách nhập kho: " + ex.Message);
                return new List<ImportNote>();
            }
        }

        public List<ImportNote> GetImportsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _conn.ImportNotes
                    .Where(x => x.importDate >= startDate && x.importDate <= endDate.AddDays(1))
                    .Include(x => x.ImportDetails)
                    .ThenInclude(x => x.Ingredient)
                    .OrderByDescending(x => x.importDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi lấy nhập kho theo khoảng ngày: " + ex.Message);
                return new List<ImportNote>();
            }
        }

        public ImportNote GetImportById(int importId)
        {
            try
            {
                return _conn.ImportNotes
                    .Where(x => x.importID == importId)
                    .Include(x => x.ImportDetails)
                    .ThenInclude(x => x.Ingredient)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi lấy nhập kho ID {importId}: " + ex.Message);
                return null;
            }
        }
    }
}