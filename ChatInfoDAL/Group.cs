using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class Group
    {
        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
