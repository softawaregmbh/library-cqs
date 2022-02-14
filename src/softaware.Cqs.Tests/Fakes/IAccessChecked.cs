namespace softaware.Cqs.Tests.Fakes
{
    /// <summary>
    /// Marker interface for access control decorator which should apply only to access checked commands/queries.
    /// </summary>
    public interface IAccessChecked
    {
        /// <summary>
        /// Just for testing purposes: The decorator sets this value to <see langword="true"/> which we can then assert in the unit tests.
        /// </summary>
        bool AccessCheckHasBeenEvaluated { get; set; }
    }
}
