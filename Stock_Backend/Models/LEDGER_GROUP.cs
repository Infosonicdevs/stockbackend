using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{//
    public class LEDGER_GROUP
    {
        public int L_group_id { get; set; }
        public string L_group_name { get; set; }
        public string L_group_name_RL { get; set; }
        public int Patrak_id { get; set; }
        public Nullable<byte> crdr_id { get; set; }
        public Nullable<int> Seqno { get; set; }
        public string Code { get; set; }
        public string User_name { get; set; }
    }
}