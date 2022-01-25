using Newtonsoft.Json;
using System.Text;

namespace Backrole.Http.Routings.Results
{
    public class JsonResult : TextResult
    {
        /// <summary>
        /// Initialize a new <see cref="JsonResult"/> instance.
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="MimeType"></param>
        /// <param name="Encoding"></param>
        public JsonResult(object Object, string MimeType = "application/json", Encoding Encoding = null)
            : base(JsonConvert.SerializeObject(Object), MimeType, Encoding)
        {
        }
    }
}
