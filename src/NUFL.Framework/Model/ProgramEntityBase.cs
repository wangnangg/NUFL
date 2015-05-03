using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public abstract class ProgramEntityBase:MarshalByRefObject
    {
        float? _susp;
        public virtual float Susp
        {
            protected set
            {
                _susp = value;
            }
            get
            {
                if (!_susp.HasValue)
                {
                    if (DirectChildrenSortedBySusp.Count > 0)
                    {
                        _susp = DirectChildrenSortedBySusp[0].Susp;
                    }
                    else
                    {
                        _susp = float.MinValue;
                    }
                }
                return _susp.Value;
            }
        }

        public virtual string DisplayName 
        {
            get { return "ProgramEntity"; } 
        }

       

        public virtual void Reset(bool recursive)
        {
            _susp = null;
            _children_sorted_by_susp = null;
            _children_sorted_by_cov = null;
            _covered_leaf_children_count = -1;
            _leaf_children_count = -1;
            if (recursive)
            {
                foreach (var child in DirectChildren)
                {
                    child.Reset(true);
                }
            }
        }


        static List<ProgramEntityBase> EmptyList = new List<ProgramEntityBase>();
       
        public virtual IEnumerable<ProgramEntityBase> DirectChildren
        {
            get 
            {
                return EmptyList;
            }
        }

        private List<ProgramEntityBase> _children_sorted_by_susp;
        public List<ProgramEntityBase> DirectChildrenSortedBySusp
        {
            get 
            {
                if(_children_sorted_by_susp == null)
                {
                    _children_sorted_by_susp = GetDirectChildrenSortedBySusp();
                }
                return _children_sorted_by_susp;
            }
        }
        protected virtual List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            return EmptyList;
        }

        private List<ProgramEntityBase> _children_sorted_by_cov;
        public List<ProgramEntityBase> DirectChildrenSortedByCov
        {
            get
            {
                if (_children_sorted_by_cov == null)
                {
                    _children_sorted_by_cov = GetDirectChildrenSortedByCov();
                }
                return _children_sorted_by_cov;
            }
        }
        protected virtual List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            return EmptyList;
        }


        //the part pertinent to coverage
        private int _covered_leaf_children_count;
        public virtual int CoveredLeafChildrenCount
        {
            get
            {
                if( _covered_leaf_children_count < 0)
                {
                    _covered_leaf_children_count = 0;
                    foreach(var child in DirectChildren)
                    {
                        _covered_leaf_children_count += child.CoveredLeafChildrenCount;
                    }
                }
                return _covered_leaf_children_count;
            }

        }
        private int _leaf_children_count;
        public virtual int LeafChildrenCount
        {
            get
            {
                if (_leaf_children_count < 0)
                {
                    _leaf_children_count = 0;
                    foreach (var child in DirectChildren)
                    {
                        _leaf_children_count += child.LeafChildrenCount;
                    }
                }
                return _leaf_children_count;
            }

        }
        public float CoveragePercent
        {
            get
            {
                if(LeafChildrenCount == 0)
                {
                    return 1;
                } else
                {
                    return (float)CoveredLeafChildrenCount / (float)LeafChildrenCount;

                }
                
            }
        }


        public Position SourcePosition { get; set; }
    }
}
