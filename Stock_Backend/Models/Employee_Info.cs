using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class Employee_Info
    {
        public int Emp_id { get; set; }
        public string Emp_code { get; set; }
        public string Emp_name { get; set; }
        public int Designation_id { get; set; }
        public DateTime DOJ { get; set; }
        public string Qualification { get; set; }
        public string E_mail { get; set; }
        public char Status { get; set; }
        public DateTime DOR { get; set; }
        public string Reason { get; set; }
        public int Outlet_id { get; set; }
        public DateTime DOB { get; set; }
        public string Mobile_no { get; set; }
        public int Cust_id { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }


    }
}