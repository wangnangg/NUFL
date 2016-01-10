using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class SourceFile
    {
        public List<Method> Methods { private set; get; }
        public string FullName { private set; get; }

        public SourceFile(string fullname)
        {
            Methods = new List<Method>();
            FullName = fullname;
        }

        public IEnumerable<InstrumentationPoint> GetPointEnumerator()
        {
            foreach(var method in Methods)
            {
                if(method.Skipped)
                {
                    continue;
                }
                foreach(var point in method.Points)
                {
                    yield return point;
                }
            }
            yield break;
        }
    }
}
