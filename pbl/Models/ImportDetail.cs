using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class ImportDetail
    {
        public int importId { get; set; }
        public int igId { get; set; }
        public required int quantityAdded { get; set; } = 0;
        public required int importPrice { get; set; } = 0;

        public virtual ImportNote ImportNote { get; set; }
        public virtual Ingredient Ingredient { get; set; }

    }
}
