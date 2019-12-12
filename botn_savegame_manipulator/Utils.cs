using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botn_savegame_manipulator
{
    class Utils
    {
        public static int IndexOf(byte[] blob, byte[] tag, int offset, byte[] mask = null, int maxOffset = -1)
        {
            if (maxOffset < 0 || blob.Length < maxOffset)
            {
                maxOffset = blob.Length;
            }
            int hit = 0;
            for (int i = offset; i < maxOffset; i++)
            {
                if (blob[i] == tag[hit] || (mask != null && mask[hit] == 0xFF))
                {
                    hit++;
                }
                else
                {
                    hit = 0;
                }

                if (tag.Length == hit)
                {
                    return i - tag.Length + 1;
                }
            }
            return -1;
        }
    }
}
