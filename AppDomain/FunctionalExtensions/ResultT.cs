namespace AppDomain.FunctionalExtensions
{
    public class Result<T>
    {
        public bool HasErrors { get; }

        public string ErrorMessage { get; }

        public T Value { get; }

        private Result(bool hasErrors, string errorMessage, T value)
        {
            HasErrors = hasErrors;
            ErrorMessage = errorMessage;
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(false, default, value);
        }

        public static Result<T> Failure(string message)
        {
            return new Result<T>(true, message, default);
        }
    }
}