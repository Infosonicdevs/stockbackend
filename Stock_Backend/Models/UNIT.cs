using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class UNIT
    {
        public int Unit_id { get; set; }
        public string Unit_name { get; set; }
        public decimal multiple_factor { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
    }
}