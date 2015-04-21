using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;

namespace NUFL.Framework.CBFL
{
    public class Coverage
    {
        List<byte> _cov_bitmap;

        public Coverage(int total = 8196)
        {
            int length_in_byte = total / 8 + 1;
            _cov_bitmap = new List<byte>(length_in_byte);
        }

        public void Cover(UInt32 uid)
        {
            int byte_pos = (int)uid / 8;
            int byte_offset = (int)uid % 8;

            if(byte_pos >= _cov_bitmap.Count)
            {
                int inc_length = InstrumentationPoint.Count / 8 - _cov_bitmap.Count + 1;
                _cov_bitmap.AddRange(new byte[inc_length]);
                Count = InstrumentationPoint.Count;
            }
            _cov_bitmap[byte_pos] |= (byte)(1 << byte_offset);


        }

        public bool IsCovered(UInt32 uid)
        {
            int byte_pos = (int)uid / 8;
            int byte_offset = (int)uid % 8;
            int val = _cov_bitmap[byte_pos] & (byte)(1 << byte_offset);

            return val > 0 ? true : false;
        }

        public int Count { private set; get; }
    }
}
