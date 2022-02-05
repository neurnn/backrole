using Backrole.Orp.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace Backrole.Orp.Meshes
{
    public class OrpMeshOptions : IOrpMeshOptions
    {
        private List<IPEndPoint> m_InitialPeers = new();
        private IOrpOptions m_Options = new OrpOptions();

        /// <summary>
        /// Initialize a new <see cref="OrpMeshOptions"/> instance.
        /// </summary>
        public OrpMeshOptions() => MapMeshMessages(m_Options);

        /// <inheritdoc/>
        public byte[] MeshNetworkId { get; set; } = Encoding.UTF8.GetBytes("ORP_DEFAULT_ID");

        /// <inheritdoc/>
        public IPEndPoint Advertisement { get; set; }

        /// <inheritdoc/>
        public IOrpOptions ProtocolOptions
        {
            get => m_Options; 
            set
            {
                if (value != null)
                    MapMeshMessages(m_Options = value);

                else
                    MapMeshMessages(m_Options = new OrpOptions());
            }
        }

        /// <summary>
        /// Map ORP Protocol Messages.
        /// </summary>
        /// <param name="Options"></param>
        private void MapMeshMessages(IOrpOptions Options)
        {
            Options.Map(typeof(OrpMeshOptions).Assembly,
                Type => Type.GetCustomAttribute<OrpMeshAttribute>() != null, true);
        }

        /// <inheritdoc/>
        public int MaxRetriesPerPeer { get; set; } = 5;

        /// <inheritdoc/>
        public IList<IPEndPoint> InitialPeers => m_InitialPeers;

        /// <inheritdoc/>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc/>
        public TimeSpan ConnectionRecoveryDelay { get; set; } = TimeSpan.FromSeconds(5);
        
        /// <inheritdoc/>
        IReadOnlyList<IPEndPoint> IOrpMeshReadOnlyOptions.InitialPeers => m_InitialPeers;

        /// <inheritdoc/>
        IOrpReadOnlyOptions IOrpMeshReadOnlyOptions.ProtocolOptions => ProtocolOptions;

    }
}
