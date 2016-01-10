using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class : ProgramEntityBase
    {
        /// <summary>
        /// instantiate
        /// </summary>
        public Class(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// The full name of the class
        /// </summary>
        public string FullName { get; set; }


        public string Name { get; set; }



        /// <summary>
        /// A list of methods that make up the class
        /// </summary>
        public List<Method> Methods { get; set; }



        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach (var method in Methods)
                {
                    if (method.Skipped)
                    {
                        continue;
                    }
                    yield return method;
                }
                yield break;
            }
        }



        public override string DisplayName
        {
            get
            {
                return Name;
            }
        }

        public SourceFile File
        {
            get
            {
                foreach(var method in Methods)
                {
                    if(method.File != null)
                    {
                        return method.File;
                    }
                }
                return null;
            }
        }

        public int? StartLine
        {
            get
            {
                foreach (var method in Methods)
                {
                    if (method.StartLine != null)
                    {
                        return method.StartLine;
                    }
                }
                return null;
            }
        }

    }
}
