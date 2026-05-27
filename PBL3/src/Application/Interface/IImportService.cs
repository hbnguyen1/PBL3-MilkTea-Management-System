using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Application.Interface
{
    public interface IImportService
    {
        bool CreateImport(int staffId, List<ImportDetail> details);
    }
}
