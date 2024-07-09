using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication10.Models
{
    public class HopDong
    {
        public int Id { get; set; }

        [Display(Name = "Mã Đối Tượng")]
        public string Ma_Dt { get; set; }

        [Display(Name = "Chi Nhánh")]
        public string Ma_Dvcs { get; set; }

        [Display(Name = "Số Hợp Đồng")]
        public string So_Hop_Dong { get; set; }

        public bool IsActive { get; set; }

        public string Ten_Dt { get; set; }

        public string Dvcs { get; set; }

        public List<SelectListItem> DvcsList { get; set; }

        public List<SelectListItem> MaDtList { get; set; }
    }
}
