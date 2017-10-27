using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses.ViewModels
{
    public class UserEditViewModel
    {
        public ICollection<RoleInfo> RoleInfos { get; set; }
        public ICollection<A1> AuthTree { get; set; }
        public UserInfo UserInfo { get; set; }
        public UserEditViewModel()
        {            
        }
        public UserEditViewModel(
            ICollection<RoleInfo> roleInfos,
            ICollection<A1> authTree ,
            UserInfo userInfo)
        {
            RoleInfos=roleInfos;
            AuthTree=authTree;
            UserInfo=userInfo;
        }
    }
}