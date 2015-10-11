using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class MemberOfGroup
    {
        public MemberOfGroup() {}
        public MemberOfGroup(int idU, int idG)
        {
            UserID = idU;
            groupID = idG;
        }
        int groupID;

        public int GroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }
        int userID;

        public int UserID
        {
            get { return userID; }
            set { userID = value; }
        }
        int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}
