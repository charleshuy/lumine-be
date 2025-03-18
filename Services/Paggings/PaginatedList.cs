using System.Text.Json.Serialization;

namespace Application.Paggings
{
    public class PaginatedList<T>
    {
        public IReadOnlyList<T> Items { get; private set; } = new List<T>();
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }

        // Parameterless constructor for deserialization
        protected PaginatedList() { }

        // Constructor with [JsonConstructor] to help System.Text.Json bind values properly
        [JsonConstructor]
        public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalCount = totalCount;
            PageSize = pageSize;
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            Items = items;
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
