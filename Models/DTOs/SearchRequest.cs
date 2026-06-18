#nullable enable
namespace Warehouse.Models.DTOs
{
    public class SearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
