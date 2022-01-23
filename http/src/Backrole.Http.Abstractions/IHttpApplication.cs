using System.Threading.Tasks;

namespace Backrole.Http.Abstractions
{
    public interface IHttpApplication
    {
        /// <summary>
        /// Invokes the application.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task InvokeAsync(IHttpContext Context);
    }
}
