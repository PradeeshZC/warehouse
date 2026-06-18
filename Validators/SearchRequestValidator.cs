#nullable enable
using FluentValidation;
using Warehouse.Models.DTOs;

namespace Warehouse.Validators
{
    public class SearchRequestValidator : AbstractValidator<SearchRequest>
    {
        public SearchRequestValidator()
        {
            RuleFor(x => x.SortDirection).Must(sd => sd == null || sd == "asc" || sd == "desc").WithMessage("SortDirection must be 'asc' or 'desc'");
            RuleFor(x => x.SortBy).MaximumLength(100);
        }
    }
}
