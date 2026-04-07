using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_DISTRIBUTION
    {
        public int SD_id { get; set; }

        public int Batch_no { get; set; }

        public int Outlet_id { get; set; }

        public DateTime Date { get; set; }

        public char Invert { get; set; }

        public int GS_pur_id { get; set; }

        public int Stock_id { get; set; }

        public decimal Pur_amt { get; set; }

        public decimal MRP { get; set; }

        public decimal Quantity { get; set; }

        public decimal Amount { get; set; }

        public string Created_by { get; set; }

        public DateTime Created_date { get; set; }

        public string Modified_by { get; set; }

        public DateTime Modified_date { get; set; }

        public char Is_new { get; set; }

        public int txt { get; set; }

        public string User_name { get; set; }
    }
}