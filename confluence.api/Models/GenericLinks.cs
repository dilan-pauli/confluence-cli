using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluence.Api.Models
{
    public class GenericLinks
    {
        public string @base { get; set; }
        public string context { get; set; }
        public string next { get; set; }
        public string self { get; set; }
        public string webui { get; set; }
    }
}
