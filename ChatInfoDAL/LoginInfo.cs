using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class LoginInfo
    {
        public LoginInfo(string n, string p)
        {
            Name = n;
            Password = p;
        }
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }
}
