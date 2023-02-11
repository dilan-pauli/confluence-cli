using System;
namespace Confluence.Api.Models
{
    public class ConfluenceArray<T>
    {
        public GenericLinks _links { get; set; }
        public int limit { get; set; }
        public List<T> results { get; set; }
        public int size { get; set; }
        public int start { get; set; }
    }
}

