using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class Position : MarshalByRefObject
    {
        public Position()
        {

        }
        public Position(SourceFile file, int startline, int startcol, int endline, int endcol)
        {
            SourceFile = file;
            StartLine = startline;
            StartColumn = startcol;
            EndLine = endline;
            EndColumn = endcol;
        }
        public SourceFile SourceFile
        {
            get;
            set;
        }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}
