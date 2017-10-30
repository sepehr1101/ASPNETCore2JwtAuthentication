using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using AuthServer.DataLayer;
using AuthServer.DataLayer.Context;
using AuthServer.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AuthServer.Services
{
    public interface IPasswordValidatorService
    {
        ErrorInfo ValidatePassword(string password);
    }
    public class PasswordValidatorService : IPasswordValidatorService
    {
        private readonly IPolicyService _policyService;
        private readonly Policy _activePolicy;
        private ErrorInfo _errorInfo;
        private string _password;
        public PasswordValidatorService(IPolicyService policyService)
        {
           _policyService=policyService;
           _activePolicy= _policyService.FindActive();
           _errorInfo=new ErrorInfo();
        }
        public ErrorInfo ValidatePassword(string password)
        {
            _password=password;            
            if(!ValidateEmptiness())
            {
                _errorInfo.Error=true;
                _errorInfo.Error="کلمه عبور نباید خالی باشد";
                return _errorInfo;
            }
            if(!ValidateMinLength())
            {
                _errorInfo.Error=true;
                _errorInfo.Error=String.Join(" ","حداقل طول کلمه عبور ",_activePolicy.MinPasswordLength,"نویسه است");
                return _errorInfo;
            }
               
            if(!ValidateDigitExistense())
            {
                _errorInfo.Error=true;
                _errorInfo.Error="لطفا حداقل یک نویسه عددی نیز در کلمه عبور خود لحاظ فرمایید";
                return _errorInfo;
            }
            if(!ValidateUpperCaseLetter())
            {
                _errorInfo.Error=true;
                _errorInfo.Error="لطفا حداقل یک نویسه با حروف کوچک انگلیسی نیز در کلمه عبور خود لحاظ فرمایید";
                return _errorInfo;
            }
            if(!ValidateLowerCaseLetter())
            {
                _errorInfo.Error=true;
                _errorInfo.Error="لطفا حداقل یک نویسه با حروف بزرگ انگلیسی نیز در کلمه عبور خود لحاظ فرمایید";
                return _errorInfo;
            }
            if(!ValidateNonAlphaNumeric())
            {
                _errorInfo.Error=true;
                _errorInfo.Error="لطفا حداقل یک نویسه غیر از اعداد و حروف انگلیسی نیز در کلمه عبور خود لحاظ فرمایید";
                return _errorInfo;
            }
            _errorInfo.HasError=false;
            return _errorInfo;
        }
        private bool ValidateEmptiness()
        {
            if(string.IsNullOrWhiteSpace(_password))
            {
                return false;
            }
            return true;
        }
        private bool ValidateMinLength()
        {           
            if(_password.Length<_activePolicy.MinPasswordLength)
            {
                return false;
            }
            return true;
        }
        private bool ValidateDigitExistense()
        {
            if(!_activePolicy.PasswordContainsNumber)
            {
                return true;
            }
            return _password.Any(c => Char.IsDigit(c));
        }
        private bool ValidateUpperCaseLetter()
        {
            if(!_activePolicy.PasswordContainsUppercase)
            {
                return true;
            }
            return _password.Any(c => Char.IsUpper(c));
        }
        private bool ValidateLowerCaseLetter()
        {
            if(!_activePolicy.PasswordContainsLowercase)
            {
                return true;
            }
            return _password.Any(c => Char.IsLower(c));
        }
        private bool ValidateNonAlphaNumeric()
        {
            if(!_activePolicy.PasswordContainsNonAlphaNumeric)
            {
                return true;
            }
            return _password.Any(c => !Char.IsLetterOrDigit(c));
        }
    }
}