using System.Net;

namespace PegasusBackend.Extentions
{
    public static class HttpStatusCodeExtentions
    {
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode) =>
            (int)statusCode >= 200 && (int)statusCode < 300;

    }
}
