using System.IO;
using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Http Opaque Stream feature interface.
    /// This downgrade the Http connection to just binary stream.
    /// </summary>
    public interface IHttpOpaqueStreamFeature
    {
        /// <summary>
        /// Test whether the request can be downgraded to opaque stream or not.
        /// </summary>
        bool CanDowngrade { get; }

        /// <summary>
        /// Downgrade the connection to opaque stream.
        /// </summary>
        /// <returns></returns>
        Task<Stream> DowngradeAsync();
    }
}
