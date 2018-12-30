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
        }

        public User(string email, string name, string sureName, string password)
        {
            Email = email;
            Name = name;
            SureName = sureName;
            Password = password;
        }

        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public string SureName { get; set; }
        public string Password { get; set; }
       
    }
}
