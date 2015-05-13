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
        ProgramEntityBase _entity;
        List<CovEntity> _cached_children;
        public IEnumerable<CovEntity> Children 
        { 
            get
            {
                if (_cached_children == null)
                {
                    _cached_children = new List<CovEntity>();
                    foreach (var child in _entity.DirectChildrenSortedBySusp)
                    {
                        var cov_entity = new CovEntity(child);
                        _cached_children.Add(cov_entity);
                        yield return cov_entity;
                    }
                    yield break;
                } else
                {
                    foreach(var child in _cached_children)
                    {
                        yield return child;
                    }
                    yield break;
                }
                
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
        public CovEntity(ProgramEntityBase entity)
        {
            _entity = entity;
        }
    }
}
