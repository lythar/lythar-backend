using System.Net;
using System.Web.Http;

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

public class BaseHttpException : HttpResponseException
{
    public BaseHttpExceptionOptions Options;

    public BaseHttpException(BaseHttpExceptionOptions options) : base(
        new HttpResponseMessage((HttpStatusCode)options.StatusCode)
        {
            Content = JsonContent.Create(options),
            ReasonPhrase = options.ErrorMessage
        }
    ) {
        Options = options;
    }

    public BaseHttpException(string code, string message, HttpStatusCode statusCode) : this(new BaseHttpExceptionOptions(code, message, statusCode)) { }
}
