using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class City_Master
    {
        public int City_id { get; set; }
        public int Taluka_id { get; set; }
        public string City { get; set; }
        public string City_RL { get; set; }
        public string User_name { get; set; }
    }
}