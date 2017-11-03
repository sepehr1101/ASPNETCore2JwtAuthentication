namespace AuthServer.DomainClasses
{
    public class Policy
    {
        public int Id { get; set; }
        public bool EnableValidIpRecaptcha { get; set; }
        public int RequireRecaptchaInvalidAttempts { get; set; }
        public int LockInvalidAttempts { get; set; }
        public int LockMin { get; set; }
        public bool IsActive { get; set; }
        public int MinPasswordLength { get; set; }
        public bool PasswordContainsNumber { get; set; }
        public bool PasswordContainsLowercase { get; set; }
        public bool PasswordContainsUppercase { get; set; }
        public bool PasswordContainsNonAlphaNumeric { get; set; }        
        public bool CanUpdateDeviceId { get; set; }
    }    
}