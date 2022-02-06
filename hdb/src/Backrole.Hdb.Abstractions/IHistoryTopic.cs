using System;

namespace Backrole.Hdb.Abstractions
{
    /// <summary>
    /// History interface.
    /// </summary>
    public interface IHistory
    {
        /// <summary>
        /// Index of the history.
        /// </summary>
        ulong Index { get; }

        /// <summary>
        /// Hash Value of the previous record.
        /// </summary>
        byte[] PrevHash { get; }

        /// <summary>
        /// Value of the record.
        /// </summary>
        byte[] Value { get; }

        /// <summary>
        /// Value Hash of the record.
        /// </summary>
        byte[] ValueHash { get; }
    }
}
