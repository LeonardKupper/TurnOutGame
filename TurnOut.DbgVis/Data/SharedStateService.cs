using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TurnOut.DbgVis.Data
{
    public class SharedStateService
    {

        private readonly object synchronizationLock = new object();

        public event EventHandler StateChanged;

        private List<string> messages = new List<string>();

        public List<string> Messages => messages.TakeLast(10).ToList();

        public void AddMessage(string msg)
        {
            lock (synchronizationLock)
            {
                messages.Add(msg);
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
