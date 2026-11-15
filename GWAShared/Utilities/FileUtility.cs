using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Utilities
{
    public class FileUtility
    {
        public static byte[] ConvertToBytes(Stream stream)
        {
            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
