using System.ComponentModel.DataAnnotations;

namespace softaware.Cqs.Tests.CQ.Contract.Commands;

public class ValidationCommand : ICommand
{
    [Required]
    public string? Value { get; set; }
}
