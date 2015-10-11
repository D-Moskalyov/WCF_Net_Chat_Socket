using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInfoDAL
{
    [Serializable]
    public class RegInfo
    {
        public RegInfo(string n, string p1, string p2)
        {
            Name = n;
            Pass1 = p1;
            Pass2 = p2;
        }
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string pass1;

        public string Pass1
        {
            get { return pass1; }
            set { pass1 = value; }
        }
        string pass2;

        public string Pass2
        {
            get { return pass2; }
            set { pass2 = value; }
        }
    }
}
