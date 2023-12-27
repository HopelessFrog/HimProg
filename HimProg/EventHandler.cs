using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HimProg
{
    public class EventDelegate
    {
        public event EventHandler SomethingChanged;

        public void InvokeThis()
        {
            SomethingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
