using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluence.Api.Models
{
    public class SpaceExpaned
    {
        public string description { get; set; }
        public string history { get; set; }
        public string homepage { get; set; }
        public string icon { get; set; }
        public string identifiers { get; set; }
        public string lookAndFeel { get; set; }
        public string metadata { get; set; }
        public string operations { get; set; }
        public string permissions { get; set; }
        public string settings { get; set; }
        public string theme { get; set; }
    }

    public class Space
    {
        public SpaceExpaned _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public long id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string type { get; set; }
    }

    public class SpaceArray
    {
        public GenericLinks _links { get; set; }
        public int limit { get; set; }
        public List<Space> results { get; set; }
        public int size { get; set; }
        public int start { get; set; }
    }
}
