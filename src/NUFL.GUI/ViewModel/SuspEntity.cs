using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;

namespace NUFL.GUI.ViewModel
{
    public class SuspEntity
    {
        const string ProgramImage = "/NUFL.GUI;Component/Images/Program.png";
        const string ModuleImage = "/NUFL.GUI;Component/Images/Module.png";
        const string ClassImage = "/NUFL.GUI;Component/Images/Class.png";
        const string MethodImage = "/NUFL.GUI;Component/Images/Method.png";
        static string[] LevelImages =new string[]{ "",
                                        "/NUFL.GUI;Component/Images/level1.png",
                                        "/NUFL.GUI;Component/Images/level2.png",
                                        "/NUFL.GUI;Component/Images/level3.png",
                                        "/NUFL.GUI;Component/Images/level4.png",
                                        "/NUFL.GUI;Component/Images/level5.png",
                                    };
        ProgramEntityBase _entity;
        public IEnumerable<SuspEntity> Children 
        { 
            get
            {
                if (this._entity.GetType() == typeof(Method))
                {
                    yield break;
                }

                foreach (var child in _entity.DirectChildrenSortedBySusp)
                {
                    var susp_entity = new SuspEntity(child);
                    yield return susp_entity;
                }
                yield break;
                
            }
        }
        public string Name
        {
            get
            {
                return _entity.DisplayName;
            }
        }
        public float Percentage
        {
            get
            {
                return (float)_entity.SuspLevel / (float)5;
            }
        }
        public string ImagePath
        {
            get
            {
                if (_entity.GetType() == typeof(Method))
                {
                    return MethodImage;
                }
                if (_entity.GetType() == typeof(Class))
                {
                    return ClassImage;
                }
                if (_entity.GetType() == typeof(Module))
                {
                    return ModuleImage;
                }
                if (_entity.GetType() == typeof(Program))
                {
                    return ProgramImage;
                }
                return null;
            }
        }

        public string LevelImagePath
        {
            get
            {
                int level = _entity.SuspLevel >= 1 && _entity.SuspLevel <= 5 ? _entity.SuspLevel : 1;
                return LevelImages[level];
            }
        }

        public Tuple<string, int> Position
        {
            get
            {
                string file_path =null;
                int? start_line = null;
                var method = _entity as Method;
                if(method != null)
                {
                    file_path = method.File != null ? method.File.FullName : null;
                    start_line = method.StartLine;
                }
                var @class = _entity as Class;
                if(@class != null)
                {
                    file_path = @class.File != null ? @class.File.FullName : null;
                    start_line = @class.StartLine;
                }
                if(file_path != null && start_line != null)
                {
                    return new Tuple<string, int>(file_path, start_line.Value);
                }
                return null;
            }
        }
        public SuspEntity(ProgramEntityBase entity)
        {
            _entity = entity;
        }
    }
}
