using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Lifetime;

namespace NUFL.Service
{
    public class GlobalInstanceServiceBase:MarshalByRefObject
    {
        public void Confirmation()
        {

        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
