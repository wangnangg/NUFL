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
        public Position(string filepath, int startline, int startcol)
        {
            SourceFile = filepath;
            StartLine = startline;
            StartColumn = startcol;
        }
        private static List<string> Files = new List<string>();
        public static int GetFileId(string filename)
        {
            if(Files.Contains(filename))
            {
                return Files.IndexOf(filename);
            }

            Files.Add(filename);
            return Files.Count - 1;
        }
        private int _file_id;
        private int _start_line;
        private int _start_column;
        public string SourceFile 
        {
            get
            {
                return Files[_file_id];
            }
            set
            {
                _file_id = GetFileId(value);
            }
        }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
    }
}
