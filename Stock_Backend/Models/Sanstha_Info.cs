using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{//
    public class Sanstha_Info
    {
        public string Sanstha_name { get; set; }
        public string Reg_no { get; set; }
        public DateTime Reg_date { get; set; }
        public string Address { get; set; }
        public int State_id { get; set; }
        public int Dist_id { get; set; }
        public int Tal_id { get; set; }
        public int City_id { get; set; }
        public string Tag_line { get; set; }
        public string Website { get; set; }
        public string Logo { get; set; }
        public char Is_logo { get; set; }
        public string GST_no { get; set; }
        public string Short_name { get; set; }

    }
}