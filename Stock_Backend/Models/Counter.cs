using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class Counter
    {
        public int Counter_id { get; set; }
        public string Counter_name { get; set; }
        public int Outlet_id { get; set; }
        public string Computer_name { get; set; }
        public string User { get; set; }
    }
}