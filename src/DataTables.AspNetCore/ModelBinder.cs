using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DataTables.Core;
using DataTables.Core.NameConvention;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     Represents a model binder for DataTables request element.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public class ModelBinder : IModelBinder
    {
        /// <summary>
        ///     Provides custom aditional parameters processing for your request.
        ///     You have to implement this to populate 'IDataTablesRequest' object with aditional (user-defined) request values.
        /// </summary>
        public Func<ModelBindingContext, IDictionary<string, object>> ParseAdditionalParameters;

        /// <summary>
        ///     Binds request data/parameters/values into a 'IDataTablesRequest' element.
        /// </summary>
        /// <param name="bindingContext">Binding context for data/parameters/values.</param>
        /// <returns>An IDataTablesRequest object or null if binding was not possible.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return Task.Factory.StartNew(() => { BindModel(bindingContext, Configuration.Options, ParseAdditionalParameters); });
        }

        /// <summary>
        ///     For internal and testing use only.
        ///     Binds request data/parameters/values into a 'IDataTablesRequest' element.
        /// </summary>
        /// <param name="bindingContext">Binding context for data/parameters/values.</param>
        /// <param name="options">DataTables.AspNet global options.</param>
        /// <param name="parseAditionalParameters"></param>
        /// <returns>An IDataTablesRequest object or null if binding was not possible.</returns>
        public virtual void BindModel(ModelBindingContext bindingContext, IOptions options, Func<ModelBindingContext, IDictionary<string, object>> parseAditionalParameters)
        {
            // Model binding is not set, thus AspNet5 will keep looking for other model binders.
            if (bindingContext.ModelType != typeof(IDataTablesRequest))
                return;

            // Binding is set to a null model to avoid unexpected errors.
            if (options?.RequestNameConvention == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var values = bindingContext.ValueProvider;

            // Accordingly to DataTables docs, it is recommended to receive/return draw casted as int for security reasons.
            // This is meant to help prevent XSS attacks.
            var draw = values.GetValue(options.RequestNameConvention.Draw);
            var _draw = 0;
            if (options.IsDrawValidationEnabled && !Parse(draw, out _draw))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var start = values.GetValue(options.RequestNameConvention.Start);
            var _start = 0;
            Parse(start, out _start);

            var length = values.GetValue(options.RequestNameConvention.Length);
            var _length = options.DefaultPageLength;
            Parse(length, out _length);

            var searchValue = values.GetValue(options.RequestNameConvention.SearchValue);
            string _searchValue = null;
            Parse(searchValue, out _searchValue);

            var searchRegex = values.GetValue(options.RequestNameConvention.IsSearchRegex);
            var _searchRegex = false;
            Parse(searchRegex, out _searchRegex);

            var search = new Search(_searchValue, _searchRegex);

            // Parse columns & column sorting.
            var columns = ParseColumns(values, options.RequestNameConvention);
            ParseSorting(columns, values, options.RequestNameConvention);

            if (options.IsRequestAdditionalParametersEnabled && (parseAditionalParameters != null))
            {
                var aditionalParameters = parseAditionalParameters(bindingContext);
                var model = new DataTablesRequest(_draw, _start, _length, search, columns, aditionalParameters);
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            else
            {
                var model = new DataTablesRequest(_draw, _start, _length, search, columns);
                bindingContext.Result = ModelBindingResult.Success(model);
            }
        }

        /// <summary>
        ///     For internal use only.
        ///     Parse column collection.
        /// </summary>
        /// <param name="values">Request parameters.</param>
        /// <param name="names">Name convention for request parameters.</param>
        /// <returns></returns>
        private static List<IColumn> ParseColumns(IValueProvider values, IRequestNameConvention names)
        {
            var columns = new List<IColumn>();

            var counter = 0;
            while (true)
            {
                // Parses Field value.
                var columnField = values.GetValue(string.Format(names.ColumnField, counter));
                string _columnField = null;
                if (!Parse(columnField, out _columnField)) break;

                // Parses Name value.
                var columnName = values.GetValue(string.Format(names.ColumnName, counter));
                string _columnName = null;
                if (!Parse(columnName, out _columnName)) break;

                // Parses Orderable value.
                var columnSortable = values.GetValue(string.Format(names.IsColumnSortable, counter));
                var _columnSortable = true;
                Parse(columnSortable, out _columnSortable);

                // Parses Searchable value.
                var columnSearchable = values.GetValue(string.Format(names.IsColumnSearchable, counter));
                var _columnSearchable = true;
                Parse(columnSearchable, out _columnSearchable);

                // Parsed Search value.
                var columnSearchValue = values.GetValue(string.Format(names.ColumnSearchValue, counter));
                string _columnSearchValue = null;
                Parse(columnSearchValue, out _columnSearchValue);

                // Parses IsRegex value.
                var columnSearchRegex = values.GetValue(string.Format(names.IsColumnSearchRegex, counter));
                var _columnSearchRegex = false;
                Parse(columnSearchRegex, out _columnSearchRegex);

                var search = new Search(_columnSearchValue, _columnSearchRegex);

                // Instantiates a new column with parsed elements.
                var column = new Column(_columnName, _columnField, _columnSearchable, _columnSortable, search);

                // Adds the column to the return collection.
                columns.Add(column);

                // Increments counter to keep processing columns.
                counter++;
            }

            return columns;
        }

        /// <summary>
        ///     For internal use only.
        ///     Parse sort collection.
        /// </summary>
        /// <param name="columns">Column collection to use when parsing sort.</param>
        /// <param name="values">Request parameters.</param>
        /// <param name="names">Name convention for request parameters.</param>
        /// <returns></returns>
        private static IEnumerable<ISort> ParseSorting(IReadOnlyCollection<IColumn> columns, IValueProvider values, IRequestNameConvention names)
        {
            var sorting = new List<ISort>();

            for (var i = 0; i < columns.Count; i++)
            {
                var sortField = values.GetValue(string.Format(names.SortField, i));
                var _sortField = 0;
                if (!Parse(sortField, out _sortField)) break;

                var column = columns.ElementAt(_sortField);

                var sortDirection = values.GetValue(string.Format(names.SortDirection, i));
                string _sortDirection = null;
                Parse(sortDirection, out _sortDirection);

                if (column.SetSort(i, _sortDirection))
                    sorting.Add(column.Sort);
            }

            return sorting;
        }

        /// <summary>
        ///     Parses a possible raw value and transforms into a strongly-typed result.
        /// </summary>
        /// <typeparam name="TElement">The expected type for result.</typeparam>
        /// <param name="value">The possible request value.</param>
        /// <param name="result">Returns the parsing result or default value for type is parsing failed.</param>
        /// <returns>True if parsing succeeded, False otherwise.</returns>
        private static bool Parse<TElement>(ValueProviderResult value, out TElement result)
        {
            result = default(TElement);

            if (string.IsNullOrWhiteSpace(value.FirstValue))
                return false;

            try
            {
                result = (TElement)Convert.ChangeType(value.FirstValue, typeof(TElement));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}