namespace Backrole.Orp.Abstractions
{
    public enum OrpMeshPeerState
    {
        None,

        /// <summary>
        /// Nothing did.
        /// </summary>
        Pending,

        /// <summary>
        /// Connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Handshaking
        /// </summary>
        Handshaking,

        /// <summary>
        /// Connected.
        /// </summary>
        Connected,

        /// <summary>
        /// Disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Removed. (not recovering)
        /// </summary>
        Removed
    }
}
