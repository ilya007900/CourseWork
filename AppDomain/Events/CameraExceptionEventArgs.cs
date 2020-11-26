namespace AppDomain.Events
{
    public class CameraExceptionEventArgs
    {
        public string Message { get; }

        public CameraExceptionEventArgs(string message)
        {
            Message = message;
        }
    }
}