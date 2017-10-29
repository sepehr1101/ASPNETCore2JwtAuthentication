namespace AuthServer.DomainClasses.ViewModels
{
    using System.Collections.Generic;
    public class UserSearchProViewModel
    {
        public ICollection<int> RoleIds;
        public ICollection<string> Claims;
    }
}