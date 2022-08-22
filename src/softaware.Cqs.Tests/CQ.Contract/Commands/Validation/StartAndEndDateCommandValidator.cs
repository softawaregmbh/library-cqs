using FluentValidation;

namespace softaware.Cqs.Tests.CQ.Contract.Commands.Validation;

public class StartAndEndDateCommandValidator : AbstractValidator<StartAndEndDateCommand>
{
    public StartAndEndDateCommandValidator()
    {
        this.RuleFor(c => c.End).GreaterThan(c => c.Start);
    }
}
