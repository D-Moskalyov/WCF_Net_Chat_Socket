using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    public class MessageManager
    {
        static List<Message> messages = new List<Message>();

        public static List<Message> Messages
        {
            get { return MessageManager.messages; }
            set { MessageManager.messages = value; }
        }

        public static void LoadMessages(SqlDataReader reader)
        {
            while (reader.Read())
            {
                Message message = new Message();

                message.MessageID = (int)reader["MessageID"];
                message.UserID = (int)reader["UserID"];
                message.GroupID = (int)reader["GroupID"];
                message.Text = reader["Texts"].ToString();
                message.DT = (DateTime)reader["TimeMes"];
                message.Status = (int)Statuses.unchanged;

                messages.Add(message);
            }
        }
    }
}
