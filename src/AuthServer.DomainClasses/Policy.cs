namespace AuthServer.DomainClasses
{
    public  class Policy
    {
        public int Id { get; set; }
        public bool EnableValidIpRecaptcha { get; set; }
        public int RequireRecaptchaInvalidAttempts { get; set; }
        public int LockInvalidAttempts { get; set; }
        public bool IsActive { get; set; }
    }    
}