namespace AuthServer.DomainClasses.ViewModels
{
    public class PasswordPolicy
    {
        public int Id { get; set; }
        public int MinPasswordLength { get; set; }
        public bool PasswordContainsNumber { get; set; }
        public bool PasswordContainsLowercase { get; set; }
        public bool PasswordContainsUppercase { get; set; }
        public bool PasswordContainsNonAlphaNumeric { get; set; }

        public PasswordPolicy()
        {
            
        }
        public PasswordPolicy(int minLengh,bool hasUpper,bool hasLower,bool hasNumber,bool hasNonAplphaNumeric)
        {
            PasswordContainsLowercase=hasLower;
            PasswordContainsNonAlphaNumeric=hasNonAplphaNumeric;
            PasswordContainsNumber= hasNumber;
        }
    }
}