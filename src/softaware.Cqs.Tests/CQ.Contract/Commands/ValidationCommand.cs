using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace softaware.Cqs.Tests.CQ.Contract.Commands
{
    public class ValidationCommand : ICommand
    {
        [Required]
        public string Value { get; set; }
    }
}
