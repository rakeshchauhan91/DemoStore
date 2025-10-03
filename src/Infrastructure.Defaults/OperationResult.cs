 
namespace Infrastructure.Defaults
{
   
    public class OperationResult<T> : OperationResult
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of failed result");

        private OperationResult(bool isSuccess, T? value, string error) : base(isSuccess, error)
        {
            _value = value;
        }

        public static OperationResult<T> Success(T value) => new(true, value, string.Empty);
        public static OperationResult<T> Failure(string error) => new(false, default, error);

        public static implicit operator OperationResult<T>(T value) => Success(value);
        public static implicit operator OperationResult<T>(string error) => Failure(error);
    }
    public class OperationResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected OperationResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error ?? string.Empty;
        }

        public static OperationResult Success() => new(true, string.Empty);
        public static OperationResult Failure(string error) => new(false, error);

        public static implicit operator OperationResult(string error) => Failure(error);
    }
}
