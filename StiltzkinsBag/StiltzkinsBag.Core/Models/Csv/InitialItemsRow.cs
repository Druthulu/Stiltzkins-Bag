using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from InitialItems.csv.
    /// Columns: ItemID;Count
    ///
    /// No Comment column — simplest format in the set.
    /// Only item IDs 236–253 (consumable pool) are valid for randomization.
    /// </summary>
    public sealed class InitialItemsRow
    {
        public int ItemID { get; set; }
        public byte Count { get; set; }
    }

    public sealed class InitialItemsRowMap : ClassMap<InitialItemsRow>
    {
        public InitialItemsRowMap()
        {
            Map(m => m.ItemID).Index(0);
            Map(m => m.Count).Index(1);
        }
    }
}