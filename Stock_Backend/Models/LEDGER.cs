using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    //
    public class LEDGER
    {
        public int Ledger_id { get; set; }
        public int Ledger_no { get; set; }
        public string Ledger_name { get; set; }
        public string Ledger_name_RL { get; set; }
        public int Ledger_group_id { get; set; }
        public int Ledger_subgroup_id { get; set; }
        public int Ledger_type { get; set; }
        public byte Is_personal { get; set; }
        public Nullable<int> Cust_type_id { get; set; }
        public byte Accountable { get; set; }
        public byte Status { get; set; }
        public string Created_by { get; set; }
        public System.DateTime Created_date { get; set; }
        public string Modified_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
    }
}