using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_GROUP
    {
        public int Group_id { get; set; }
        public string Group_name { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
    }
}