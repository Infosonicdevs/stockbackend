using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class Dist_Master
    {
        public int Dist_id { get; set; }
        public int State_id { get; set; }
        public string Dist { get; set; }
        public string Dist_RL { get; set; }
        public string User_name { get; set; }

    }
}