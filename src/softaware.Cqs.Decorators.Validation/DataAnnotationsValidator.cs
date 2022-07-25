#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.ComponentModel.DataAnnotations;

namespace softaware.Cqs.Decorators.Validation;

/// <summary>
/// Validates annotations by using <see cref="Validator.ValidateObject(object, ValidationContext, bool)"/>.
/// </summary>
public class DataAnnotationsValidator : IValidator
{
    void IValidator.ValidateObject(object instance)
    {
        var context = new ValidationContext(instance, null, null);
        // Throws an exception when instance is invalid.
        Validator.ValidateObject(instance, context, validateAllProperties: true);
    }
}
