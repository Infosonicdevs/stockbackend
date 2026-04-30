using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_BALANCE
    {
        public int Bal_id { get; set; }
        public int Stock_id { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Outlet_id { get; set; }

        public string Created_By { get; set; }
        public string Modified_By { get;set; }
    }
}