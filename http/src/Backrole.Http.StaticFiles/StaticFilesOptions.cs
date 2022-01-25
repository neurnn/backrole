using Backrole.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Backrole.Http.StaticFiles
{
    public class StaticFilesOptions
    {
        private List<Func<IHttpRequest, FileInfo, Task<bool>>> m_Filters = new();
        private List<Func<IHttpContext, FileInfo, Task>> m_Prependers = new();
        private string m_BasePath = "";

        /// <summary>
        /// Initialize a new <see cref="StaticFilesOptions"/> instance.
        /// </summary>
        public StaticFilesOptions()
        {
            Directory = new DirectoryInfo(Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "wwwroot"));
        }

        /// <summary>
        /// Base Path to map.
        /// </summary>
        public string BasePath
        {
            get => m_BasePath;
            set => m_BasePath = Normalize(value ?? "");
        }

        /// <summary>
        /// Directory to provide. (default: approot/wwwroot)
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Adds a prepender delegate that prepend headers for the static file.
        /// </summary>
        /// <param name="Prepends"></param>
        /// <returns></returns>
        public StaticFilesOptions Use(Func<IHttpContext, FileInfo, Task> Prepends)
        {
            m_Prependers.Add(Prepends);
            return this;
        }

        /// <summary>
        /// Add a filter delegate that decide the physical file can be provided or not.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public StaticFilesOptions UseFilter(Func<IHttpRequest, FileInfo, Task<bool>> Filter)
        {
            m_Filters.Add(Filter);
            return this;
        }

        /// <summary>
        /// Normalize the path that is matchable
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        private static string Normalize(string Path) 
        { 
            var Names = Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var Stack = new List<string>();

            foreach (var Each in Names)
            {
                if (Each == ".")
                    continue;

                if (Each == "..")
                {
                    if (Stack.Count > 0)
                        Stack.RemoveAt(Stack.Count - 1);

                    continue;
                }

                Stack.Add(Uri.UnescapeDataString(Each));
            }
            
            return string.Join('/', Stack);
        }

        /// <summary>
        /// Translate the requested file to.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        internal async Task<FileInfo> Translate(IHttpRequest Request)
        {
            var Target = Normalize(Request.PathString);
            var BasePath = $"{m_BasePath}/".TrimStart('/');

            if (BasePath.Length <= 0 || Target.StartsWith(BasePath))
            {
                var Subpath = Target.Substring(BasePath.Length).Trim('/');
                var Realpath = Path.Combine(Directory.FullName, Subpath);

                if (File.Exists(Realpath))
                {
                    var FileInfo = new FileInfo(Realpath);

                    foreach (var Each in m_Filters)
                    {
                        if (!await Each(Request, FileInfo))
                            return null;
                    }

                    return FileInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Prepend something to the static files response.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        internal async Task PrependAsync(IHttpContext Http, FileInfo File)
        {
            foreach (var Each in m_Prependers)
                await Each(Http, File);
        }
    }
}
