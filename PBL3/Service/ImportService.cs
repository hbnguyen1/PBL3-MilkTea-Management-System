using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.Data;
using PBL3.Models;
using PBL3.Core;
using PBL3.Interface;

namespace PBL3.Service
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

                        // Tạo phiếu nhập
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
    }
}