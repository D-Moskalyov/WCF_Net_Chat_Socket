using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    public class UserManager
    {
        static List<User> users = new List<User>();

        public static List<User> Users
        {
            get { return UserManager.users; }
            set { UserManager.users = value; }
        }

        public static void LoadUsers(SqlDataReader reader)
        {
            while (reader.Read())
            {
                User user = new User();

                user.UserID = (int)reader["UserID"];
                user.NickName = reader["NickName"].ToString();
                user.Password = reader["Pass"].ToString();
                user.Ban = (DateTime)reader["Ban"];
                user.Status = (int)Statuses.unchanged;

                users.Add(user);
            }
        }
    }
}
