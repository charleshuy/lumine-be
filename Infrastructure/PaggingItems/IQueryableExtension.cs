using Application.Paggings;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.PagingItems
{
    public static class IQueryableExtension
    {
        public static async Task<PaginatedList<T>> GetPaginatedList<T>(this IQueryable<T> source, int pageIndex, int pageSize) where T : class
            => await IQueryableExtension.CreateAsync(source.AsNoTracking(), pageIndex, pageSize);

        public static Task<List<TDestination>> ProjectToListAsync<TDestination>(this IQueryable queryable, IConfigurationProvider configuration) where TDestination : class
            => queryable.ProjectTo<TDestination>(configuration).AsNoTracking().ToListAsync();

        public static async Task<PaginatedList<T>> CreateAsync<T>(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PaginatedList<T> Create<T>(List<T> source, int pageNumber, int pageSize)
        {
            var totalCount = source.Count;
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}
