using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from ShopItems.csv.
    /// Columns: Comment;Id;Items
    ///
    /// Items is a comma-separated int[] of item IDs. May be empty (shops 0023/0024).
    /// </summary>
    public sealed class ShopItemsRow
    {
        /// <summary>Shop label (e.g. "Shop 0000").</summary>
        public string Comment { get; set; } = string.Empty;

        public int Id { get; set; }

        /// <summary>Item IDs stocked by this shop. Empty array for empty shops.</summary>
        public int[] Items { get; set; } = [];
    }

    public sealed class ShopItemsRowMap : ClassMap<ShopItemsRow>
    {
        public ShopItemsRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Items).Index(2).TypeConverter<IntArrayConverter>();
        }
    }
}