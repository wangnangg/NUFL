using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;

namespace NUFL.Framework.Analysis
{
    public class Coverage
    {
        List<byte> _cov_bitmap;
        Program _program;

        public Coverage(Program program, int total = 8196)
        {
            int length_in_byte = total / 8 + 1;
            _cov_bitmap = new List<byte>(length_in_byte);
            _program = program;
        }

        public void Cover(UInt32 uid)
        {
            int byte_pos = (int)uid / 8;
            int byte_offset = (int)uid % 8;

            if(byte_pos >= _cov_bitmap.Count)
            {
                int points_count = _program.Points.Count;
                int inc_length = points_count / 8 - _cov_bitmap.Count + 1;
                _cov_bitmap.AddRange(new byte[inc_length]);
                Count = points_count;
            }
            _cov_bitmap[byte_pos] |= (byte)(1 << byte_offset);


        }

        public bool IsCovered(UInt32 uid)
        {
            int byte_pos = (int)uid / 8;
            int byte_offset = (int)uid % 8;
            if(byte_pos >= _cov_bitmap.Count)
            {
                return false;
            }
            int val = _cov_bitmap[byte_pos] & (byte)(1 << byte_offset);

            return val > 0 ? true : false;
        }

        public int Count { private set; get; }
    }
}
