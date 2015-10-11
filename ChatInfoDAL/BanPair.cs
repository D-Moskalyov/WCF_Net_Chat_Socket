using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    [Serializable]
    public class BanPair
    {
        int id1;

        public int Id1
        {
            get { return id1; }
            set { id1 = value; }
        }
        int id2;

        public int Id2
        {
            get { return id2; }
            set { id2 = value; }
        }
        int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}
