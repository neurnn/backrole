namespace Backrole.Orp.Abstractions
{
    public enum OrpMeshState
    {
        /// <summary>
        /// Doing nothing.
        /// </summary>
        None,

        /// <summary>
        /// Running.
        /// </summary>
        Normal,

        /// <summary>
        /// Discoverying the remote peers.
        /// </summary>
        Discovering,

        /// <summary>
        /// Refreshing the peer manager.
        /// </summary>
        Refreshing
    }
}
