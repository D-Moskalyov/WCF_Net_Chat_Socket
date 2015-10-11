using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLib
{
    [Serializable]
    public class MainEvent
    {
        public event EventHandler<MyEventArgs> SomeEnent;

        public void OnSomeEvent(int t)
        {
            MyEventArgs myEventArgs = new MyEventArgs(t);
            if (SomeEnent != null)
            {
                SomeEnent(this, myEventArgs);
            }
        }
    }
}
