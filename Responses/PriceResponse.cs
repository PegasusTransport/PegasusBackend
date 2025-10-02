namespace PegasusBackend.Responses
{
    public record PriceResponse
    {
        public bool Success { get; init; }
        public decimal? Price { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
