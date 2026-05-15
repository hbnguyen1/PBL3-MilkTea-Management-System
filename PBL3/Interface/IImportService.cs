using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    public interface IImportService
    {
        bool CreateImport(int staffId, List<ImportDetail> details);
    }
}
