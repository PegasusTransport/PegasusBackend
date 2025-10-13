using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Responses;
using System.Net;

namespace PegasusBackend.Helpers.StatusMapper
{
    public static class Generate
    {
        public static ActionResult<T> ActionResult<T>(ServiceResponse<T> response)
        {
            return response.StatusCode switch
            {
                // 2xx Success
                HttpStatusCode.OK => new OkObjectResult(new
                {
                    data = response.Data,
                    message = response.Message
                }),
                HttpStatusCode.Created => new CreatedResult(response.Message, response.Data),
                HttpStatusCode.Accepted => new AcceptedResult(response.Message, response.Data),
                HttpStatusCode.NonAuthoritativeInformation => new ObjectResult(response.Data) { StatusCode = 203 },
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.ResetContent => new ObjectResult(response.Data) { StatusCode = 205 },
                HttpStatusCode.PartialContent => new ObjectResult(response.Data) { StatusCode = 206 },

                // 3xx Redirection
                HttpStatusCode.MultipleChoices => new ObjectResult(response.Data) { StatusCode = 300 },
                HttpStatusCode.MovedPermanently => new ObjectResult(response.Data) { StatusCode = 301 },
                HttpStatusCode.Found => new ObjectResult(response.Data) { StatusCode = 302 },
                HttpStatusCode.SeeOther => new ObjectResult(response.Data) { StatusCode = 303 },
                HttpStatusCode.NotModified => new ObjectResult(response.Data) { StatusCode = 304 },
                HttpStatusCode.UseProxy => new ObjectResult(response.Data) { StatusCode = 305 },
                HttpStatusCode.TemporaryRedirect => new ObjectResult(response.Data) { StatusCode = 307 },
                HttpStatusCode.PermanentRedirect => new ObjectResult(response.Data) { StatusCode = 308 },

                // 4xx Client Errors
                HttpStatusCode.BadRequest => new BadRequestObjectResult(new
                {
                    data = response.Data,
                    message = response.Message
                }),
                HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(new
                {
                    data = response.Data,
                    message = response.Message
                }),
                HttpStatusCode.PaymentRequired => new ObjectResult(response.Data) { StatusCode = 402 },
                HttpStatusCode.Forbidden => new ObjectResult(response.Data) { StatusCode = 403 },
                HttpStatusCode.NotFound => new NotFoundObjectResult(response.Data),
                HttpStatusCode.MethodNotAllowed => new ObjectResult(response.Data) { StatusCode = 405 },
                HttpStatusCode.NotAcceptable => new ObjectResult(response.Data) { StatusCode = 406 },
                HttpStatusCode.ProxyAuthenticationRequired => new ObjectResult(response.Data) { StatusCode = 407 },
                HttpStatusCode.RequestTimeout => new ObjectResult(response.Data) { StatusCode = 408 },
                HttpStatusCode.Conflict => new ConflictObjectResult(response.Data),
                HttpStatusCode.Gone => new ObjectResult(response.Data) { StatusCode = 410 },
                HttpStatusCode.LengthRequired => new ObjectResult(response.Data) { StatusCode = 411 },
                HttpStatusCode.PreconditionFailed => new ObjectResult(response.Data) { StatusCode = 412 },
                HttpStatusCode.RequestEntityTooLarge => new ObjectResult(response.Data) { StatusCode = 413 },
                HttpStatusCode.RequestUriTooLong => new ObjectResult(response.Data) { StatusCode = 414 },
                HttpStatusCode.UnsupportedMediaType => new ObjectResult(response.Data) { StatusCode = 415 },
                HttpStatusCode.RequestedRangeNotSatisfiable => new ObjectResult(response.Data) { StatusCode = 416 },
                HttpStatusCode.ExpectationFailed => new ObjectResult(response.Data) { StatusCode = 417 },
                HttpStatusCode.MisdirectedRequest => new ObjectResult(response.Data) { StatusCode = 421 },
                HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(response.Data),
                HttpStatusCode.Locked => new ObjectResult(response.Data) { StatusCode = 423 },
                HttpStatusCode.FailedDependency => new ObjectResult(response.Data) { StatusCode = 424 },
                HttpStatusCode.UpgradeRequired => new ObjectResult(response.Data) { StatusCode = 426 },
                HttpStatusCode.PreconditionRequired => new ObjectResult(response.Data) { StatusCode = 428 },
                HttpStatusCode.TooManyRequests => new ObjectResult(response.Data) { StatusCode = 429 },
                HttpStatusCode.RequestHeaderFieldsTooLarge => new ObjectResult(response.Data) { StatusCode = 431 },
                HttpStatusCode.UnavailableForLegalReasons => new ObjectResult(response.Data) { StatusCode = 451 },

                // 5xx Server Errors
                HttpStatusCode.InternalServerError => new ObjectResult(response.Data) { StatusCode = 500 },
                HttpStatusCode.NotImplemented => new ObjectResult(response.Data) { StatusCode = 501 },
                HttpStatusCode.BadGateway => new ObjectResult(response.Data) { StatusCode = 502 },
                HttpStatusCode.ServiceUnavailable => new ObjectResult(response.Data) { StatusCode = 503 },
                HttpStatusCode.GatewayTimeout => new ObjectResult(response.Data) { StatusCode = 504 },
                HttpStatusCode.HttpVersionNotSupported => new ObjectResult(response.Data) { StatusCode = 505 },
                HttpStatusCode.VariantAlsoNegotiates => new ObjectResult(response.Data) { StatusCode = 506 },
                HttpStatusCode.InsufficientStorage => new ObjectResult(response.Data) { StatusCode = 507 },
                HttpStatusCode.LoopDetected => new ObjectResult(response.Data) { StatusCode = 508 },
                HttpStatusCode.NotExtended => new ObjectResult(response.Data) { StatusCode = 510 },
                HttpStatusCode.NetworkAuthenticationRequired => new ObjectResult(response.Data) { StatusCode = 511 },

                // Default fallback for any unhandled status codes
                _ => new ObjectResult(response.Message) { StatusCode = (int)response.StatusCode }
            };
        }
    }
}