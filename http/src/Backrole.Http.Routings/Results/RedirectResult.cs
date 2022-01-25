using Backrole.Http.Abstractions;
using Backrole.Http.Routings.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.Routings.Results
{
    public class RedirectResult : IHttpResult
    {
        private bool m_Permanent;
        private string m_Location;

        /// <summary>
        /// Initialize a new <see cref="RedirectResult"/> instance.
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="Permanent"></param>
        public RedirectResult(string Location, bool Permanent = false)
        {
            m_Location = Location;
            m_Permanent = Permanent;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Http)
        {
            Http.Response.Status = m_Permanent ? 301 : 302;
            Http.Response.StatusPhrase = null;

            Http.Response.Headers.Set("Location", m_Location ?? "/");
            return Task.CompletedTask;
        }
    }
}
