using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication10.Models
{
    public class Kh_HD
    {
        public int Id { get; set; }

        public string Ma_Dt { get; set; }
        public string Ma_dt { get; internal set; }
        public string Ma_Dvcs { get; set; }
        public string Ma_dvcs { get; internal set; }
        public string So_Hop_Dong { get; set; }

        public int IsActive { get; set; }
    }
}