using System;
using System.Text;
using System.Threading.Tasks;
using DataTables.Core;
using Microsoft.AspNetCore.Mvc;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     Represents a custom JsonResult that can process IDataTablesResponse accordingly to settings.
    /// </summary>
    public class DataTablesJsonResult : IActionResult
    {
        /// <summary>
        ///     Defines the default result content type.
        /// </summary>
        private const string DefaultContentType = "application/json; charset={0}";

        /// <summary>
        ///     Defines the default json request behavior.
        /// </summary>
        private const bool AllowJsonThroughHttpGet = false;

        /// <summary>
        ///     Defines the default result enconding.
        /// </summary>
        private static readonly Encoding DefaultContentEncoding = Encoding.UTF8;

        private readonly bool _allowGet;
        private readonly Encoding _contentEncoding;

        private readonly string _contentType;
        private readonly object _data;

        public DataTablesJsonResult(IDataTablesResponse response) : this(response, DefaultContentType, DefaultContentEncoding, AllowJsonThroughHttpGet)
        {
        }

        public DataTablesJsonResult(IDataTablesResponse response, bool allowJsonThroughHttpGet)
            : this(response, DefaultContentType, DefaultContentEncoding, allowJsonThroughHttpGet)
        {
        }

        public DataTablesJsonResult(IDataTablesResponse response, string contentType, Encoding contentEncoding, bool allowJsonThroughHttpGet)
        {
            _data = response;
            _contentEncoding = contentEncoding ?? DefaultContentEncoding;
            _contentType = string.Format(contentType ?? DefaultContentType, _contentEncoding.WebName);
            _allowGet = allowJsonThroughHttpGet;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (!_allowGet && context.HttpContext.Request.Method.ToUpperInvariant().Equals("GET"))
                throw new NotSupportedException(
                    "This request has been blocked because sensitive information could be disclosed to third party web sites when this is used in a GET request. To allow GET requests, set JsonRequestBehavior to AllowGet.");

            var response = context.HttpContext.Response;

            response.ContentType = _contentType;

            if (_data != null)
            {
                var content = _data.ToString();
                var contentBytes = _contentEncoding.GetBytes(content);
                await response.Body.WriteAsync(contentBytes, 0, contentBytes.Length);
            }
        }
    }
}