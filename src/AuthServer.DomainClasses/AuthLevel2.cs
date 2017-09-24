using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLevel2
    {
        public AuthLevel2()
        {           
           Children=new HashSet<AuthLevel3>();
        }

        public int Id { get; set; }
        public int AuthLeve1Id { get; set; }
        public string IconClass { get; set; }
        public string Title { get; set; }
        public string ElementId { get; set; }

        public virtual AuthLevel1 Parent { get; set; }
        public virtual ICollection<AuthLevel3> Children { get; set; }
    }
}
