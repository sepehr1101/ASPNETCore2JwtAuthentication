using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLeve1
    {
        public AuthLeve1()
        {           
           Children=new HashSet<AuthLeve2>();
        }

        public int Id { get; set; }

        public int  AppBoundaryCode { get; set; } 
        public int AppBoundaryTitle { get; set; }
        public virtual ICollection<AuthLeve2> Children { get; set; }
    }
}
