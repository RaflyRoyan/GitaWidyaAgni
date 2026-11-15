using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Models
{
    public class EnumItem
    {
        public int EnumId { get; set; }
        public string EnumName { get; set; } = string.Empty;
        public string EnumDescription { get; set; } = string.Empty;
    }
}
