using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluence.Api.Models
{
    public class Page : ConfluenceResponse
    {
        public string status { get; set; }
        public string title { get; set; }
        public string spaceId { get; set; }
        public string parentId { get; set; }
        public string authorId { get; set; }
        public DateTime createdAt { get; set; }
        public VersionV2 version { get; set; }
        public BodyV2 body { get; set; }
    }

    public class BodyV2
    {
        public string storage { get; set; }
        public string atlas_doc_format { get; set; }
    }
}
