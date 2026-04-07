using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class VENDOR_BAL
    {
        public int Opn_bal_id { get; set; }
        public int Vend_id { get; set; }
        public int Outlet_id { get; set; }
        public decimal Amount { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }

    }
}