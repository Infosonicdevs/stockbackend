using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class VOUCHER
    {
        public TRANS Trans { get; set; }
        public List<TRANS_DETAILS> Trans_Details { get; set; } = new List<TRANS_DETAILS>();
    }
}