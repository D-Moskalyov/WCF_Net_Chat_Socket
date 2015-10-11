using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    public class MemberOfGroupManager
    {
        static List<MemberOfGroup> membersOfGroup = new List<MemberOfGroup>();

        public static List<MemberOfGroup> MembersOfGroup
        {
            get { return MemberOfGroupManager.membersOfGroup; }
            set { MemberOfGroupManager.membersOfGroup = value; }
        }

        public static void LoadMembersOfGroup(SqlDataReader reader)
        {
            while (reader.Read())
            {
                MemberOfGroup memberOfGroup = new MemberOfGroup();

                memberOfGroup.UserID = (int)reader["UserID"];
                memberOfGroup.GroupID = (int)reader["GroupID"];
                memberOfGroup.Status = (int)Statuses.unchanged;

                membersOfGroup.Add(memberOfGroup);
            }
        }
    }
}
