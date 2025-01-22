using Ambev.DeveloperEvaluation.Domain.Models;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation
{
    public class SaleValidator : AbstractValidator<Sale>
    {
        public SaleValidator()
        {
            RuleFor(sale => sale.SaleNumber).NotEmpty();
            RuleFor(sale => sale.Customer).NotEmpty();
            RuleFor(sale => sale.Branch).NotEmpty();
            RuleForEach(sale => sale.Products).SetValidator(new SaleItemValidator());
        }
    }

    public class SaleItemValidator : AbstractValidator<SaleItem>
    {
        public SaleItemValidator()
        {
            RuleFor(item => item.Name).NotEmpty();
            RuleFor(item => item.Quantity).GreaterThan(0);
            RuleFor(item => item.UnitPrice).GreaterThan(0);
        }
    }
}
