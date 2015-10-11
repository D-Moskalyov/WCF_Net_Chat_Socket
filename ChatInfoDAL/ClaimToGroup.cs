using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class ClaimToGroup
    {
        public ClaimToGroup(ChatInfoDAL.User us, Group gr)
        {
            User = us;
            Group = gr;
        }
        ChatInfoDAL.User user;

        public ChatInfoDAL.User User
        {
            get { return user; }
            set { user = value; }
        }
        Group group;

        public Group Group
        {
            get { return group; }
            set { group = value; }
        }
        public override string ToString()
        {
            return String.Format("{0} - {1}", User, Group);
        }
    }
}
