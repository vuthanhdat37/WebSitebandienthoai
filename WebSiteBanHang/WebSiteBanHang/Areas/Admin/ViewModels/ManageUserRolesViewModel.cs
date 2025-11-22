using System.Collections.Generic;

namespace WebSiteBanHang.Areas.Admin.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public List<string> Roles { get; set; }
    }
} 