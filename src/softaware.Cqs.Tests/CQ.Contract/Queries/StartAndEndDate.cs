﻿namespace softaware.Cqs.Tests.CQ.Contract.Queries;

public class StartAndEndDate : IQuery<bool>
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
