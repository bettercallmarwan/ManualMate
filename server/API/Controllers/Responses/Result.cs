using System.Net;

namespace ManualMate.API.Controllers.Responses
{
    public class Result<T>
    {
        public bool Success { get; }
        public T Value { get; }
        public string Error { get; }
        public HttpStatusCode? StatusCode { get; set; }
        private Result(bool success, T value, string error, HttpStatusCode? statusCode = null)
        {
            Success = success;
            Value = value;
            Error = error;
            StatusCode = statusCode;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null, HttpStatusCode.OK);
        public static Result<T> Fail(string error, HttpStatusCode? statusCode = null) => new Result<T>(false, default, error, statusCode);
    }
}