namespace webapp
{
    public struct ClientMessage
    {
        public string ClientId { get; set; }
        public string Message { get; set; }

        public ClientMessage(string clientId, string message)
        {
            ClientId = clientId;
            Message = message;
        }
    }
}