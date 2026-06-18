#nullable enable
using FluentValidation;
using Warehouse.Models.DTOs;

namespace Warehouse.Validators
{
    public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public PaginationRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
            RuleFor(x => x.SortDirection).Must(sd => sd == null || sd == "asc" || sd == "desc").WithMessage("SortDirection must be 'asc' or 'desc'");
        }
    }
}
