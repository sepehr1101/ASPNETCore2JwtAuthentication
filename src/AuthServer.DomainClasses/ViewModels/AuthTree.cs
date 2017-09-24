using System;
using System.Collections.Generic;
namespace AuthServer.DomainClasses.ViewModels
{
    public class BaseA
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
    public class A1:BaseA
    {       
        public ICollection<A2> A2s { get; set; }
    }
    public class A2:BaseA
    {
        public string IconClass { get; set; }
        public ICollection<A3> A3s { get; set; }
    }
    public class A3 :BaseA
    {
        public ICollection<A4> A4s { get; set; }
    }
    public class A4:BaseA
    {
        public string ClaimValue { get; set; }
    }
}