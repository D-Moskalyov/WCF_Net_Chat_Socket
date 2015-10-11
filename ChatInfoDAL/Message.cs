using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class Message
    {
        int messageID;

        public int MessageID
        {
            get { return messageID; }
            set { messageID = value; }
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
        string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        DateTime dT;

        public DateTime DT
        {
            get { return dT; }
            set { dT = value; }
        }
        int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}
