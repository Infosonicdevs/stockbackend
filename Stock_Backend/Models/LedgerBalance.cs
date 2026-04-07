using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class LedgerBalance
    {
        public int Opn_bal_id { get; set; }
        public int L_id { get; set; }
        public decimal Amt {  get; set; }
        public int Outlet_id { get; set; }
        public string User_name { get; set; }

    }
}