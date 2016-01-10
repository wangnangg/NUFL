using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
namespace NUFL.GUI.ViewModel
{
    public class CovEntity
    {
        const string ProgramImage = "/NUFL.GUI;Component/Images/Program.png";
        const string ModuleImage = "/NUFL.GUI;Component/Images/Module.png";
        const string ClassImage = "/NUFL.GUI;Component/Images/Class.png";
        const string MethodImage = "/NUFL.GUI;Component/Images/Method.png";
        
        ProgramEntityBase _entity;
        public IEnumerable<CovEntity> Children 
        { 
            get
            {
                if (this._entity.GetType() == typeof(Method))
                {
                    yield break;
                }


                foreach (var child in _entity.DirectChildrenSortedByCov)
                {
                    var cov_entity = new CovEntity(child);
                    yield return cov_entity;
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
                return _entity.CoveragePercent;
            }
        }

        public string PercentageString
        {
            get
            {
                return string.Format("{0}%", (int)(Percentage * 100));
            }
        }
        public string ImagePath
        {
            get
            {
                if(_entity.GetType() == typeof(Method))
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

        public Tuple<string, int> Position
        {
            get
            {
                string file_path = null;
                int? start_line = null;
                var method = _entity as Method;
                if (method != null)
                {
                    file_path = method.File != null ? method.File.FullName : null;
                    start_line = method.StartLine;
                }
                var @class = _entity as Class;
                if (@class != null)
                {
                    file_path = @class.File != null ? @class.File.FullName : null;
                    start_line = @class.StartLine;
                }
                if (file_path != null && start_line != null)
                {
                    return new Tuple<string, int>(file_path, start_line.Value);
                }
                return null;
            }
        }
        public CovEntity(ProgramEntityBase entity)
        {
            _entity = entity;
        }
    }
}
