namespace PegasusBackend.Responses;

public record ServiceResponse<T>(
    bool Success,
    T? Data,
    string Message
)

{
    public static ServiceResponse<T> SuccessResponse(T data, string message = "OK")
    {
        return new ServiceResponse<T>(true, data, message);
    }

    public static ServiceResponse<T> FailResponse(string message)
    {
        return new ServiceResponse<T>(false, default, message);
    }

}
