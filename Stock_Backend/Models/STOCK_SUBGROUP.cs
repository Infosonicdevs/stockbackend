using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_SUBGROUP
    {
        public int Subgroup_id { get; set; }
        public int Group_id { get; set; }
        public string Subgroup_name { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }

    }
}