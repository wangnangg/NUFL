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
        ProgramEntityBase _entity;
        List<SuspEntity> _cached_children;
        public IEnumerable<SuspEntity> Children 
        { 
            get
            {
                if (_cached_children == null)
                {
                    _cached_children = new List<SuspEntity>();
                    foreach (var child in _entity.DirectChildrenSortedBySusp)
                    {
                        var susp_entity = new SuspEntity(child);
                        _cached_children.Add(susp_entity);
                        yield return susp_entity;
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
                return _entity.Susp;
            }
        }
        public SuspEntity(ProgramEntityBase entity)
        {
            _entity = entity;
        }
    }
}
