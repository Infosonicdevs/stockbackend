using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finex_api.Models
{
    public class YEAR_INFO
    {
        public int Year_id { get; set; }
        public System.DateTime Start_date { get; set; }
        public System.DateTime End_date { get; set; }
        public int Status { get; set; }
        public string Password { get; set; }
        public string User_name { get; set; }
    }
}