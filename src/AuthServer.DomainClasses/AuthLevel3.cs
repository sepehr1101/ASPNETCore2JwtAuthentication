using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLevel3
    {
        public AuthLevel3()
        {           
           Children=new HashSet<AuthLevel4>();
        }

        public int Id { get; set; }
        public int AuthLevel2Id { get; set; }
         public string Domain { get; set; }
        public string PreRoute { get; set; }
        public string Parameters { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string ElementId { get; set; }
        public virtual AuthLevel2 Parent { get; set; }
        public virtual ICollection<AuthLevel4> Children { get; set; }
    }
}
