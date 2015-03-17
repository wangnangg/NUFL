using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Persistance.CBFL
{
    public class CBFLPoint
    {
        private static readonly List<CBFLPoint> CBFLPoints;
        static CBFLPoint()
        {
            CBFLPoints = new List<CBFLPoint>(8192) { null };
        }

        int _executed_passed = 0;

        public int ExecutedPassed
        {
            get { return _executed_passed; }
            set { _executed_passed = value; }
        }

        int _executed_failed = 0;

        public int ExecutedFailed
        {
            get { return _executed_failed; }
            set { _executed_failed = value; }
        }

        int _skipped_passed = 0;

        public int SkippedPassed
        {
            get { return _skipped_passed; }
            set { _skipped_passed = value; }
        }

        int _skipped_failed = 0;

        public int SkippedFailed
        {
            get { return _skipped_failed; }
            set { _skipped_failed = value; }
        }

    }
}
