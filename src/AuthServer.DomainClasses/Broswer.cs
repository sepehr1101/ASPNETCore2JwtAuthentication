using System.Collections.Generic;
namespace AuthServer.DomainClasses
{
    public class Browser
    {
        public Browser()
        {
            Logins=new HashSet<Login>();
        }
        public int Id { get; set; }
        public string TitleEng { get; set; }
        public string TitleFa { get; set; }

        public virtual ICollection<Login> Logins { get; set; }
    }
}