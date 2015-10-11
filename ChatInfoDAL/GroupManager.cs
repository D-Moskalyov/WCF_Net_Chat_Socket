using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    public class GroupManager
    {
        static List<Group> groups = new List<Group>();

        public static List<Group> Groups
        {
            get { return GroupManager.groups; }
            set { GroupManager.groups = value; }
        }

        public static void LoadGroups(SqlDataReader reader)
        {
            while (reader.Read())
            {
                Group group = new Group();

                group.Id = (int)reader["GroupID"];
                group.Title = reader["GroupName"].ToString();
                group.Status = (int)Statuses.unchanged;

                groups.Add(group);
            }
        }
    }
}
