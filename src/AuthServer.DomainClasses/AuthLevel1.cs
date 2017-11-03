using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLevel1
    {
        public AuthLevel1()
        {           
           Children=new HashSet<AuthLevel2>();
        }

        public int Id { get; set; }

        public int  AppBoundaryCode { get; set; } 
        public string AppBoundaryTitle { get; set; }
        public bool InSidebar { get; set; }
        public virtual ICollection<AuthLevel2> Children { get; set; }        
    }
}
