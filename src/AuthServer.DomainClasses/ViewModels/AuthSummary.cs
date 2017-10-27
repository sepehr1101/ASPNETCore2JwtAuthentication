using System;

namespace AuthServer.DomainClasses.ViewModels
{
    public class AuthSummary
    {
        public int _1Id { get; set; }
        public int _2Id { get; set; }
        public int _3Id { get; set; }
        public int _4Id { get; set; }
        public string _1Title { get; set; }
        public string _2Title { get; set; }
        public string _3Title { get; set; }
        public string _4Title { get; set; }        
		public string PreRoute {get;set;}
		public string  Controller {get;set;}
		public string  Action {get;set;}
		public string Parameters {get;set;}
        public string IconClass { get; set; }
        public string ClaimValue { get; set; }
    }

    public class AuthSummaryAll:AuthSummary
    {
        public bool IsSelected { get; set; }
    }
}