using FluentValidation;

namespace softaware.Cqs.Tests.CQ.Contract.Queries.Validation;

public class StartAndEndDateValidator : AbstractValidator<StartAndEndDate>
{
    public StartAndEndDateValidator()
    {
        this.RuleFor(c => c.End).GreaterThan(c => c.Start);
    }
}
