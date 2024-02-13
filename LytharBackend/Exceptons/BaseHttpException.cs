using System.Net;

namespace LytharBackend.Exceptons;

public class BaseHttpExceptionOptions
{
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public BaseHttpExceptionOptions(string code, string message, HttpStatusCode statusCode) {
        ErrorCode = code;
        ErrorMessage = message;
        StatusCode = (int)statusCode;
    }
}

public class BaseHttpException : Exception
{
    public BaseHttpExceptionOptions Options;
    public HttpResponseMessage ResponseMessage;

    public BaseHttpException(BaseHttpExceptionOptions options) : base(options.ErrorMessage) {
        Options = options;
        ResponseMessage = new HttpResponseMessage((HttpStatusCode)options.StatusCode)
        {
            Content = JsonContent.Create(options),
            ReasonPhrase = options.ErrorMessage
        };
    }

    public BaseHttpException(string code, string message, HttpStatusCode statusCode) : this(new BaseHttpExceptionOptions(code, message, statusCode)) { }
}
