using System.Collections.Generic;

namespace Backrole.Crypto.Abstractions
{
    public interface IAlgorithmProvider
    {
        /// <summary>
        /// Names of the supported algorithms.
        /// </summary>
        IEnumerable<string> Supports { get; }

        /// <summary>
        /// Get the instance of the target algorithm if supported.
        /// Otherwise this returns null.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        IAlgorithm Get(string Name);
    }

    public interface IAlgorithmProvider<TAlgorithm> : IAlgorithmProvider where TAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Get the instance of <typeparamref name="TAlgorithm"/> if supported.
        /// Otherwise this returns null.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        new TAlgorithm Get(string Name);
    }
}
