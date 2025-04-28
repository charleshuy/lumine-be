using Application.Paggings;
using AutoMapper;

namespace Application.MappingProfiles
{
    public class PaginatedListConverter<TSource, TDestination> : ITypeConverter<PaginatedList<TSource>, PaginatedList<TDestination>>
    {
        public PaginatedList<TDestination> Convert(PaginatedList<TSource> source, PaginatedList<TDestination> destination, ResolutionContext context)
        {
            var items = context.Mapper.Map<List<TDestination>>(source.Items);

            return new PaginatedList<TDestination>(
                items,
                source.TotalCount,
                source.PageNumber,
                source.PageSize
            );
        }
    }
}
