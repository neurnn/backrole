namespace Backrole.Crypto.Abstractions
{
    /// <summary>
    /// Creates <see cref="ISignAlgorithm"/> instance.
    /// </summary>
    public interface ISignAlgorithmProvider : IAlgorithmProvider<ISignAlgorithm>
    {
    }
}
