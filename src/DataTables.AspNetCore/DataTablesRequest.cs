using System.Collections.Generic;
using DataTables.Core;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     For internal use only.
    ///     Represents a DataTables request.
    /// </summary>
    internal class DataTablesRequest : IDataTablesRequest
    {
        public DataTablesRequest(int draw, int start, int length, ISearch search, IEnumerable<IColumn> columns)
            : this(draw, start, length, search, columns, null)
        {
        }

        public DataTablesRequest(int draw, int start, int length, ISearch search, IEnumerable<IColumn> columns, IDictionary<string, object> additionalParameters)
        {
            Draw = draw;
            Start = start;
            Length = length;
            Search = search;
            Columns = columns;
            AdditionalParameters = additionalParameters;
        }

        public IDictionary<string, object> AdditionalParameters { get; }
        public IEnumerable<IColumn> Columns { get; }
        public int Draw { get; }
        public int Length { get; }
        public ISearch Search { get; }
        public int Start { get; }
    }
}