using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_RATE
    {
        public int Rate_id { get; set; }
        public int Stock_id { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal Rate { get; set; }
        public DateTime Change_date { get; set; }
        public int Sequence_no { get; set; }
        public string On_form { get; set; }
        public string User_name { get; set; }
    }
}