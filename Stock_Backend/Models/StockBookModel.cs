using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class StockBookModel
    {
        public int Stock_id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? Outlet_id { get; set; }
    }
}