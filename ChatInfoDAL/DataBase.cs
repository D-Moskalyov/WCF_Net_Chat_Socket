using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ChatInfoDAL
{
    enum Statuses { unchanged, changed, creation, remove };

    public class DataBase
    {
        static SqlConnection sql;

        public static SqlConnection Sql
        {
            get { return DataBase.sql; }
        }

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ChatInfoConnection"].ConnectionString;
        }

        public static SqlConnection GetSqlConnection()
        {
            sql = new SqlConnection(GetConnectionString());
            try
            {
                sql.Open();
            }
            catch (System.Data.SqlClient.SqlException e)
            {

            }
            return sql;
        }

        public static void BeforeClosing()
        {
            if (sql != null && sql.State == ConnectionState.Open)
                sql.Close();
        }

        public static bool GetStateConnection()
        {
            if (sql != null && sql.State == ConnectionState.Open)
                return true;
            return false;
        }

        public static void SaveChange()
        {
            if (!(DataBase.Sql.State == ConnectionState.Open))
            {
                if (DataBase.GetSqlConnection().State == ConnectionState.Open)
                {
                    //ClientsManager.UpdateClients();
                    //SectionsManager.UpdateSections();
                    //Goodsmanager.UpdateGoods();
                }
            }
        }
    }
}
