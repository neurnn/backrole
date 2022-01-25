using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Results
{
    public class StatusResult : IHttpResult
    {
        private int m_Status = 200;
        private IHttpResult m_Content;

        /// <summary>
        /// Initialize a new <see cref="StatusResult"/> instance.
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="Content"></param>
        public StatusResult(int Status, IHttpResult Content = null)
        {
            m_Status = Status;
            m_Content = Content;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Http)
        {
            Http.Response.Status = m_Status;
            Http.Response.StatusPhrase = null;

            if (m_Content != null)
                return m_Content.InvokeAsync(Http);

            return Task.CompletedTask;
        }
    }
}
