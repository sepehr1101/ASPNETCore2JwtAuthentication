using System;
using AutoMapper;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;

namespace AuthServer.WebApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {           
            CreateMap<RegisterUserViewModel,User>();
            CreateMap<Role,RoleInfo>();
            CreateMap<User,UserDisplayViewModel>();
            CreateMap<UserClaim,UserClaimViewModel>();
            CreateMap<User,UserInfo>();
            CreateMap<Policy,PasswordPolicy>();
            CreateMap<Login,LoginViewModel>();
            CreateMap<User,UserValueKey>();
        }
    }
}