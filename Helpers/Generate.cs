using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Responses;
using System.Net;

namespace PegasusBackend.Helpers
{
    public static class Generate
    {
        public static ActionResult<T> ActionResult<T>(ServiceResponse<T> serviceResponse)
        {
            var apiResponse = new
            {
                data = serviceResponse.Data,
                message = serviceResponse.Message
            };
            
            return serviceResponse.StatusCode switch
            {
                // 2xx Success
                HttpStatusCode.OK => new OkObjectResult(apiResponse),
                HttpStatusCode.Created => new CreatedResult(serviceResponse.Message, apiResponse),
                HttpStatusCode.Accepted => new AcceptedResult(serviceResponse.Message, apiResponse),
                HttpStatusCode.NonAuthoritativeInformation => new ObjectResult(apiResponse) { StatusCode = 203 },
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.ResetContent => new ObjectResult(apiResponse) { StatusCode = 205 },
                HttpStatusCode.PartialContent => new ObjectResult(apiResponse) { StatusCode = 206 },

                // 3xx Redirection
                HttpStatusCode.MultipleChoices => new ObjectResult(apiResponse) { StatusCode = 300 },
                HttpStatusCode.MovedPermanently => new ObjectResult(apiResponse) { StatusCode = 301 },
                HttpStatusCode.Found => new ObjectResult(apiResponse) { StatusCode = 302 },
                HttpStatusCode.SeeOther => new ObjectResult(apiResponse) { StatusCode = 303 },
                HttpStatusCode.NotModified => new ObjectResult(apiResponse) { StatusCode = 304 },
                HttpStatusCode.UseProxy => new ObjectResult(apiResponse) { StatusCode = 305 },
                HttpStatusCode.TemporaryRedirect => new ObjectResult(apiResponse) { StatusCode = 307 },
                HttpStatusCode.PermanentRedirect => new ObjectResult(apiResponse) { StatusCode = 308 },

                // 4xx Client Errors
                HttpStatusCode.BadRequest => new BadRequestObjectResult(apiResponse),
                HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(apiResponse),
                HttpStatusCode.PaymentRequired => new ObjectResult(apiResponse) { StatusCode = 402 },
                HttpStatusCode.Forbidden => new ObjectResult(apiResponse) { StatusCode = 403 },
                HttpStatusCode.NotFound => new NotFoundObjectResult(apiResponse),
                HttpStatusCode.MethodNotAllowed => new ObjectResult(apiResponse) { StatusCode = 405 },
                HttpStatusCode.NotAcceptable => new ObjectResult(apiResponse) { StatusCode = 406 },
                HttpStatusCode.ProxyAuthenticationRequired => new ObjectResult(apiResponse) { StatusCode = 407 },
                HttpStatusCode.RequestTimeout => new ObjectResult(apiResponse) { StatusCode = 408 },
                HttpStatusCode.Conflict => new ConflictObjectResult(apiResponse),
                HttpStatusCode.Gone => new ObjectResult(apiResponse) { StatusCode = 410 },
                HttpStatusCode.LengthRequired => new ObjectResult(apiResponse) { StatusCode = 411 },
                HttpStatusCode.PreconditionFailed => new ObjectResult(apiResponse) { StatusCode = 412 },
                HttpStatusCode.RequestEntityTooLarge => new ObjectResult(apiResponse) { StatusCode = 413 },
                HttpStatusCode.RequestUriTooLong => new ObjectResult(apiResponse) { StatusCode = 414 },
                HttpStatusCode.UnsupportedMediaType => new ObjectResult(apiResponse) { StatusCode = 415 },
                HttpStatusCode.RequestedRangeNotSatisfiable => new ObjectResult(apiResponse) { StatusCode = 416 },
                HttpStatusCode.ExpectationFailed => new ObjectResult(apiResponse) { StatusCode = 417 },
                HttpStatusCode.MisdirectedRequest => new ObjectResult(apiResponse) { StatusCode = 421 },
                HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(apiResponse),
                HttpStatusCode.Locked => new ObjectResult(apiResponse) { StatusCode = 423 },
                HttpStatusCode.FailedDependency => new ObjectResult(apiResponse) { StatusCode = 424 },
                HttpStatusCode.UpgradeRequired => new ObjectResult(apiResponse) { StatusCode = 426 },
                HttpStatusCode.PreconditionRequired => new ObjectResult(apiResponse) { StatusCode = 428 },
                HttpStatusCode.TooManyRequests => new ObjectResult(apiResponse) { StatusCode = 429 },
                HttpStatusCode.RequestHeaderFieldsTooLarge => new ObjectResult(apiResponse) { StatusCode = 431 },
                HttpStatusCode.UnavailableForLegalReasons => new ObjectResult(apiResponse) { StatusCode = 451 },

                // 5xx Server Errors
                HttpStatusCode.InternalServerError => new ObjectResult(apiResponse) { StatusCode = 500 },
                HttpStatusCode.NotImplemented => new ObjectResult(apiResponse) { StatusCode = 501 },
                HttpStatusCode.BadGateway => new ObjectResult(apiResponse) { StatusCode = 502 },
                HttpStatusCode.ServiceUnavailable => new ObjectResult(apiResponse) { StatusCode = 503 },
                HttpStatusCode.GatewayTimeout => new ObjectResult(apiResponse) { StatusCode = 504 },
                HttpStatusCode.HttpVersionNotSupported => new ObjectResult(apiResponse) { StatusCode = 505 },
                HttpStatusCode.VariantAlsoNegotiates => new ObjectResult(apiResponse) { StatusCode = 506 },
                HttpStatusCode.InsufficientStorage => new ObjectResult(apiResponse) { StatusCode = 507 },
                HttpStatusCode.LoopDetected => new ObjectResult(apiResponse) { StatusCode = 508 },
                HttpStatusCode.NotExtended => new ObjectResult(apiResponse) { StatusCode = 510 },
                HttpStatusCode.NetworkAuthenticationRequired => new ObjectResult(apiResponse) { StatusCode = 511 },

                // Default fallback for any unhandled status codes
                _ => new ObjectResult(apiResponse) { StatusCode = (int)serviceResponse.StatusCode }
            };
        }
    }
}