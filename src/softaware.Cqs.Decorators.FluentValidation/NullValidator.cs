using FluentValidation;

namespace softaware.Cqs.Decorators.FluentValidation
{
    /// <summary>
    /// Implementation of the "Null Object Pattern". This validator does not validate anything.
    /// </summary>
    public class NullValidator<T> : AbstractValidator<T>
    {
    }
}
