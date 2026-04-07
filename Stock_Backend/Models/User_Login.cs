using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class User_Login
    {
        public int User_id { get; set; }
        public int Emp_id { get; set; }
        public string User_name { get; set; }
        public string Password { get; set; }
        public int Role_id { get; set; }
        public char Log_in {  get; set; }
        public char Status { get; set; }
        public string Created_by { get; set; }
    }
}