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
    public class BanPairManager
    {
        static List<BanPair> banPairs = new List<BanPair>();

        public static List<BanPair> BanPairs
        {
            get { return BanPairManager.banPairs; }
            set { BanPairManager.banPairs = value; }
        }

        public static void LoadBanPairs(SqlDataReader reader)
        {
            while (reader.Read())
            {
                BanPair banPair = new BanPair();

                banPair.Id1 = (int)reader["UserID1"];
                banPair.Id2 = (int)reader["UserID2"];
                banPair.Status = (int)Statuses.unchanged;

                banPairs.Add(banPair);
            }
        }
    }
}
