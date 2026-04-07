using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class GST_SLAB
    {
        public int Id { get; set; }
        public int Tax_code { get; set; }
        public string Heading {  get; set; }
        public decimal CGST_per {  get; set; }
        public decimal SGST_per { get; set; }
        public decimal IGST_per { get; set; }
        public int CGST_l_id { get; set; }
        public int SGST_l_id { get; set; }
        public int IGST_l_id { get;set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
    }
}