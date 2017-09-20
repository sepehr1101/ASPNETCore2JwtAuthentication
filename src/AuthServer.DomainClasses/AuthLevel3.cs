using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLeve3
    {
        public AuthLeve3()
        {           
           Children=new HashSet<AuthLeve4>();
        }

        public int Id { get; set; }
        public int AuthLeve2Id { get; set; }
         public string Domain { get; set; }
        public string PreRoute { get; set; }
        public string Parameters { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string ElementId { get; set; }
        public virtual AuthLeve2 Parent { get; set; }
        public virtual ICollection<AuthLeve4> Children { get; set; }
    }
}
