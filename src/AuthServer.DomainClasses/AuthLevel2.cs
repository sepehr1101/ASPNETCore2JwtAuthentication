using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLeve2
    {
        public AuthLeve2()
        {           
           Children=new HashSet<AuthLeve3>();
        }

        public int Id { get; set; }
        public int AuthLeve1Id { get; set; }
        public string IconClass { get; set; }
        public string Title { get; set; }
        public string ElementId { get; set; }

        public virtual AuthLeve1 Parent { get; set; }
        public virtual ICollection<AuthLeve3> Children { get; set; }
    }
}
