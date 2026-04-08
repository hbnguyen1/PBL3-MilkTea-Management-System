using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PBL3.Models
{
    public class ImportNote
    {
        public int importNoteID { get; set; } = 0;
        public required DateTime importDate { get; set; }
        public required int staffID { get; set; } = 0;
        public required int totalCost { get; set; } = 0;
        public virtual ICollection<ImportDetail>  ImportDetails { get; set; } = new List<ImportDetail>();
    }
}
