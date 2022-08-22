using System.ComponentModel.DataAnnotations;

namespace softaware.Cqs.Tests.CQ.Contract.Queries;

public class GetSquare : IQuery<int>
{
    public GetSquare(int value)
    {
        this.Value = value;
    }

    [Range(1, 10)]
    public int Value { get; set; }
}
