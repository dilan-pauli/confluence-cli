using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluence.Api.Models
{
    public class InlineComment : ConfluenceResponse
    {
        public string status { get; set; }
        public string title { get; set; }
        public string pageId { get; set; }
        public VersionV2 version { get; set; }
        public BodyV2 body { get; set; }
    }

    public class FooterComment : InlineComment
    {

    }
}
