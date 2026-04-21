namespace Utilities.Responses
{
    public interface IResult
    {
        bool IsSuccess { get; }
        IEnumerable<string> Messages { get; }

    }
    public class Result : IResult
    {

        public bool IsSuccess { get; }
        public IEnumerable<string> Messages { get; } = [];
        protected Result(bool success)
        {
            IsSuccess = success;
        }
        protected Result(bool success, IEnumerable<string> messages)
        {
            Messages = messages;
        }
        public static IResult Success(string message) => new Result(true,
            [message]);
        public static IResult Success() => new Result(true);
        public static IResult Fail(IEnumerable<string> messages) => new Result(false, messages);
    }
}
