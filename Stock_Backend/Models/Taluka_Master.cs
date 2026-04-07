using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class Taluka_Master
    {
        public int Taluka_id { get; set; }
        public int Dist_id { get; set; }
        public string Taluka { get; set; }
        public string Taluka_RL { get; set; }
        public string User_name { get; set; }

    }
}