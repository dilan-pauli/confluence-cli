using System;
namespace Confluence.Api.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Body
    {
        //public Expandable _expandable { get; set; }
        public Storage storage { get; set; }
    }

    public class History
    {
        //public Expandable _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public User createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public bool latest { get; set; }
    }

    public class Content
    {
        //public Expandable _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public Body body { get; set; }
        public History history { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public Version version { get; set; }
    }

    public class Storage
    {
        //public Expandable _expandable { get; set; }
        //public List<object> embeddedContent { get; set; }
        //public string representation { get; set; }
        public string value { get; set; }
    }

    public class ContentArray
    {
        public GenericLinks _links { get; set; }
        public int limit { get; set; }
        public List<Content> results { get; set; }
        public int size { get; set; }
        public int start { get; set; }
    }
}

