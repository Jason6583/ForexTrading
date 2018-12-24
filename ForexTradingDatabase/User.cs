using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForexTradingDatabase
{
    [Serializable]
    public class User
    {

        public User()
        {
            PortFolio = new List<PortFolioData>();
        }

        public User(string email, string name, string sureName, string password)
        {
            Email = email;
            Name = name;
            SureName = sureName;
            Password = password;
            PortFolio = new List<PortFolioData>();
        }

        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public string SureName { get; set; }
        public string Password { get; set; }
        public List<PortFolioData> PortFolio { get; set; }
       
    }
}
