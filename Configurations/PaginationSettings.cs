namespace PegasusBackend.Configurations
{
    public class PaginationSettings
    {
        public int DefaultPage { get; set; }
        public int DefaultPageSize { get; set; }
        public int MaxPageSize { get; set; }
        public string DefaultSortOrder { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
    }

    public enum SortOrder
    {
        Asc,
        Desc
    }
}
