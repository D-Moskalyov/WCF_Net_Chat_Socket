using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatInfoDAL;

namespace Server
{
    public class MessageQueue
    {
        public MessageQueue(ChatInfoDAL.User us, string mess)
        {
            User = us;
            Message = mess;
        }
        ChatInfoDAL.User user;

        public ChatInfoDAL.User User
        {
            get { return user; }
            set { user = value; }
        }
        string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
