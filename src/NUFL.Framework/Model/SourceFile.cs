using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class SourceFile:MarshalByRefObject
    {
        public List<Method> Methods { private set; get; }
        public string FullName { private set; get; }

        private SourceFile(string fullname)
        {
            Methods = new List<Method>();
            FullName = fullname;
        }

        public static Dictionary<string, SourceFile> FileDict { get; private set; }

        static SourceFile()
        {
            FileDict = new Dictionary<string, SourceFile>();
        }

        public static SourceFile GetSourceFile(string fullname)
        {
            if( FileDict.ContainsKey(fullname) )
            {
                return FileDict[fullname];
            } 
            var file = new SourceFile(fullname);
            FileDict[fullname] = file;
            return file;
        }
    }
}
