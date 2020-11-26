namespace AppDomain.FunctionalExtensions
{
    public class Result
    {
        public bool HasErrors { get; }

        public string ErrorMessage { get; }

        private Result(bool hasErrors, string errorMessage)
        {
            HasErrors = hasErrors;
            ErrorMessage = errorMessage;
        }

        public static Result Success()
        {
            return new Result(false, default);
        }

        public static Result Failure(string message)
        {
            return new Result(true, message);
        }
    }
}