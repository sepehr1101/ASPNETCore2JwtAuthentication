using System.Collections.Generic;
namespace AuthServer.DomainClasses
{
    public class OS
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string IconClass { get; set; }
        public bool AcceptRequestFrom { get; set; }
        public virtual ICollection<Login> Logins{get;set;}
    }
}