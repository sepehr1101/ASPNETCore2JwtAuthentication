namespace AuthServer.DomainClasses.ViewModels
{
    public class SimpleMessageResponse
    {
        public string Message { get; set; }
        public SimpleMessageResponse()
        {            
        }
        public SimpleMessageResponse(string message)
        {
            Message=message;
        }
    }
}