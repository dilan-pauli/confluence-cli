using System;
namespace Confluence.Api.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Body
    {
        //public Expandable _expandable { get; set; }
        public Storage storage { get; set; }
    }

    public class Extensions
    {
        public int position { get; set; }
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
        public Extensions extensions { get; set; }
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
}

