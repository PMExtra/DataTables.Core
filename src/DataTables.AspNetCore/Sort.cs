using DataTables.Core;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     Represents sort/ordering for columns.
    /// </summary>
    public class Sort : ISort
    {
        /// <summary>
        ///     Creates a new sort instance.
        /// </summary>
        /// <param name="order">Sort order for multi-sorting.</param>
        /// <param name="direction">Sort direction</param>
        public Sort(int order, string direction)
        {
            Order = order;

            Direction = (direction ?? "").ToLowerInvariant().Equals(Configuration.Options.RequestNameConvention.SortDescending)
                ? SortDirection.Descending // Descending sort should be explicitly set.
                : SortDirection.Ascending; // Default (when set or not) is ascending sort.
        }

        /// <summary>
        ///     Gets sort direction.
        /// </summary>
        public SortDirection Direction { get; }

        /// <summary>
        ///     Gets sort order.
        /// </summary>
        public int Order { get; }
    }
}