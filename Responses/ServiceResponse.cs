using System.Net;

namespace PegasusBackend.Responses;

public class ServiceResponse<T>
{
    public HttpStatusCode StatusCode { get; }
    public T? Data { get; }
    public string Message { get; }

    private ServiceResponse(HttpStatusCode statusCode, T? data, string message)
    {
        StatusCode = statusCode;
        Data = data;
        Message = message;
    }

    public static ServiceResponse<T> SuccessResponse(HttpStatusCode statusCode,T data, string message = "OK") =>
        new ServiceResponse<T>(statusCode, data, message);

    public static ServiceResponse<T> FailResponse(HttpStatusCode statusCode, string message) =>
        new ServiceResponse<T>(statusCode, default, message);
}
