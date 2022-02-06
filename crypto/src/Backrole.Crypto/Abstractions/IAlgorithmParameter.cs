namespace Backrole.Crypto.Abstractions
{
    public interface IAlgorithmParameter
    {
        /// <summary>
        /// Indicates whether the <see cref="IAlgorithmParameter"/> is valid or not.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Name of the algorithm.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Value.
        /// </summary>
        byte[] Value { get; }
    }
}