using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace softaware.Cqs.Decorators.Validation
{
    public class DataAnnotationsValidator : IValidator
    {
        private readonly IServiceProvider container;

        public DataAnnotationsValidator(IServiceProvider container)
        {
            this.container = container;
        }

        void IValidator.ValidateObject(object instance)
        {
            var context = new ValidationContext(instance, null, null);
            // Throws an exception when instance is invalid.
            Validator.ValidateObject(instance, context, validateAllProperties: true);
        }
    }
}
