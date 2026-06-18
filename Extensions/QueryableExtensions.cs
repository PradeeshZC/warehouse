#nullable enable
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Warehouse.Models.DTOs;

namespace Warehouse.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var total = await query.CountAsync(cancellationToken);
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = total,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchTerm, Expression<Func<T, bool>>? predicate)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || predicate == null)
                return query;

            // Basic example: apply predicate if provided
            return query.Where(predicate);
        }
    }
}
