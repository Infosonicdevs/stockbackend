using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{//
    public class LEDGER_SUBGROUP
    {
        public int Ledger_subgroup_id { get; set; }
        public string Ledger_subgroup_name { get; set; }
        public string Ledger_subgroup_name_RL { get; set; }
        public int Ledger_group_id { get; set; }
        public Nullable<int> Seqno { get; set; }
        public string Code { get; set; }
        public string User_name { get; set; }
    }
}