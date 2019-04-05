namespace AuthServer.DomainClasses.ViewModels
{
    public class SimpleMessageCodeResponse
    {
        public string Message { get; set; }
        public int Code {get;set;}   
        public SimpleMessageCodeResponse()
        {            
        }
        public SimpleMessageCodeResponse(string message,int code)
        {
            Message=message;
            Code=code;
        }
    }
}