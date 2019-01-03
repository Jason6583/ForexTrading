using System;
using System.ComponentModel.DataAnnotations;

namespace ForexTradingDatabase
{

    /// <summary>
    /// Class for user in database
    /// </summary>
    [Serializable]
    public class User
    {

        public User()
        {
        }

        /// <summary>
        /// Constructor for user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="sureName"></param>
        /// <param name="password"></param>
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
