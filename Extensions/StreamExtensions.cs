using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TendalProject.Extensions
{
        public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}