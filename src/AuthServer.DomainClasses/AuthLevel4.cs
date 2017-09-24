using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLevel4
    {
        public AuthLevel4()
        {           
          
        }

        public int Id { get; set; }
        public int AuthLevel3Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public virtual AuthLevel3 Parent { get; set; }
    }
}
