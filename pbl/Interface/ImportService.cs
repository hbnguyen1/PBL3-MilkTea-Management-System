using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.Data;
using PBL3.Models;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class ImportService
    {
        public bool CreateImport(int staffId, List<ImportDetail> details)
        {
            using (var conn = new MilkTeaDBContext())
            {
                using (var transaction = conn.Database.BeginTransaction())
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

                        conn.ImportNotes.Add(note);
                        conn.SaveChanges();

                        Logger.Info($"Tạo phiếu nhập ID {note.importID} bởi staff {staffId}");

                        // Xử lý từng nguyên liệu
                        foreach (var d in details)
                        {
                            d.importID = note.importID;

                            conn.ImportDetails.Add(d);

                            var ig = conn.Ingredients.Find(d.igId);

                            if (ig != null)
                            {
                                ig.igCount += d.quantityAdded;

                                Logger.Info($"Nhập {d.quantityAdded} {ig.igName} (giá {d.importPrice})");
                            }
                        }

                        conn.SaveChanges();
                        transaction.Commit();

                        Logger.Info($"Hoàn tất nhập kho phiếu {note.importID}, tổng tiền {totalCost}");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        // Ghi vào file log
                        Logger.Error("Lỗi nhập kho: " + ex.Message);

                        // In thẳng ra màn hình Console để bạn đọc được ngay
                        Console.WriteLine("LỖI CHI TIẾT: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("SQL Server báo: " + ex.InnerException.Message);
                        }

                        return false;
                    }
                }
            }
        }
    }
}