using System;
namespace AuthServer.DomainClasses.ViewModels
{
   public class RoleInfo
   {
       public int Id { get; set; }
       public string TitleEng { get; set; }
       public string TitleFa { get; set; }
       public bool IsSelected { get; set; }
   }
}