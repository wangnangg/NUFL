using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Service
{
    interface IService
    {
       
    }

    public class GlobalServiceBase : MarshalByRefObject, IService
    {

    }
}
