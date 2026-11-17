using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH230134_MyStore.Models.ViewModel
{
    //Lưu thông tin hiển thị lên menu cá nhân trên header
    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public int CartCount { get; set; }
    }
}