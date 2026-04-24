using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class System_User_Login
    {
        public int Emp_id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Date {  get; set; }
        public int Outlet_id { get; set; }
    }
}