using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLib
{
    public class MyEventArgs : EventArgs
    {
        public MyEventArgs(int t)
        {
            this.t = t;
        }
        public int t;
    }
}