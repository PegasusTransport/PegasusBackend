namespace PegasusBackend.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = [];

        public static ApiResponse<T> Ok(T data, string message = "Success") => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

        public static ApiResponse<T> Error(string message, List<string>? errors = null) => new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? []
        };
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success") => new()
        {
            Success = true,
            Message = message
        };

    }
}
