namespace UserInfoApp.Client.Exceptions
{
    public class UserInfoAppException : Exception
    {
        public int StatusCode { get; }
        public UserInfoAppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
        public UserInfoAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
