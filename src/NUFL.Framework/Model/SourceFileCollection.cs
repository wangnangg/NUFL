using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class SourceFileCollection
    {
        Dictionary<string, SourceFile> _dict = new Dictionary<string, SourceFile>();

        public SourceFile GetFile(string fullname, bool create = true)
        {
            string fullname_small_letter = fullname.ToLower();
            if (_dict.ContainsKey(fullname_small_letter))
            {
                return _dict[fullname_small_letter];
            }
            SourceFile file = null;
            if (create)
            {
                file = new SourceFile(fullname_small_letter);
                _dict[fullname_small_letter] = file;
            }
            return file;
        }
    }
}
