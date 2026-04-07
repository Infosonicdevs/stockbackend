using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class OUTLET
    {
        public int Outlet_id { get; set; }
        public string Outlet_name { get; set; }
        public string Outlet_code { get; set; }
        public string Outlet_add { get; set; }
        public int City_id { get; set; }
        public int Taluka_id { get; set; }
        public int District_id { get; set; }
        public int State_id { get; set; }
        public string Contact_no { get; set; }
        public string Short_name { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_date { get; set; }
        public string Modified_by { get; set; }
        public DateTime Modified_date { get;set; }
        public int Is_main_branch { get; set; }  
       
    }
}