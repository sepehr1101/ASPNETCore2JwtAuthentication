namespace AuthServer.Services
{
    public class BearerTokensOptions
    {
        public string Key { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
        public int AccessTokenExpirationMinutes { set; get; }
        public int RefreshTokenExpirationMinutes { set; get; }
    }
}