using System;

namespace Backrole.Crypto.Abstractions
{
    public interface IAlgorithm
    {
        /// <summary>
        /// Name of the algorithm.
        /// </summary>
        string Name { get; }
    }
}
