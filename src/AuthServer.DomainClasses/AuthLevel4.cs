using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses
{
    public class AuthLeve4
    {
        public AuthLeve4()
        {           
          
        }

        public int Id { get; set; }
        public int AuthLeve3Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public virtual AuthLeve3 Parent { get; set; }
    }
}
