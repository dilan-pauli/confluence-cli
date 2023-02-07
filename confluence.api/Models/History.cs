using System;
namespace Confluence.Api.Models
{
    public class User
    {
        //public Expandable _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public string accountId { get; set; }
        public string accountType { get; set; }
        public string displayName { get; set; }
        public string email { get; set; }
        public bool isExternalCollaborator { get; set; }
        //public Icon profilePicture { get; set; }
        public string publicName { get; set; }
        public string type { get; set; }
    }

    public class Version
    {
        //public Expandable _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public User by { get; set; }
        public string confRev { get; set; }
        public bool contentTypeModified { get; set; }
        public string friendlyWhen { get; set; }
        public string message { get; set; }
        public bool minorEdit { get; set; }
        public int number { get; set; }
        public string syncRev { get; set; }
        public string syncRevSource { get; set; }
        public DateTime when { get; set; }
    }

    public class ContentHistory
    {
        //public Expandable _expandable { get; set; }
        public GenericLinks _links { get; set; }
        public User createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public Version lastUpdated { get; set; }
        public bool latest { get; set; }
        public Version previousVersion { get; set; }
    }
}

