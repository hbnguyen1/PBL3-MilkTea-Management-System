using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Application.Interface
{
    public interface IImportService
    {
        bool CreateImport(int staffId, List<ImportDetail> details);
        List<ImportNote> GetAllImports();
        List<ImportNote> GetImportsByDateRange(DateTime startDate, DateTime endDate);
        ImportNote GetImportById(int importId);
    }
}
