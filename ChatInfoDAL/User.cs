using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class User
    {
        public User() { }
        public User(int id, string name, string pass, DateTime dt)
        {
            UserID = id;
            NickName = name;
            Password = pass;
            Ban = dt;
        }

        int userID;

        public int UserID
        {
            get { return userID; }
            set { userID = value; }
        }
        string nickName;

        public string NickName
        {
            get { return nickName; }
            set { nickName = value; }
        }
        string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        DateTime ban;

        public DateTime Ban
        {
            get { return ban; }
            set { ban = value; }
        }
        int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }
        public override string ToString()
        {
            return NickName;
        }
    }
}
