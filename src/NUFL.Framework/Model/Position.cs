using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class Position
    {
        public Position()
        {

        }
        public Position(string filepath, int startline, int startcol)
        {
            FileId = GetFileId(filepath);
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
        public int FileId;
        public int StartLine;
        public int StartColumn;
    }
}
