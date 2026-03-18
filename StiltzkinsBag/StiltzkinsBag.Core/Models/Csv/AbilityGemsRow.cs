using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from AbilityGems.csv.
    /// Columns: Comment;Id;Gems;BoostedVersion(s)
    ///
    /// BoostedVersions is a comma-separated int[] and may be empty (e.g. "Auto-Reflect;0;15;").
    /// </summary>
    public sealed class AbilityGemsRow
    {
        /// <summary>Ability name label (e.g. "Auto-Reflect").</summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>Support ability index (0–63).</summary>
        public int Id { get; set; }

        /// <summary>Number of gem slots this ability costs to equip.</summary>
        public int Gems { get; set; }

        /// <summary>
        /// Optional list of boosted variant IDs. Empty array when the column is blank.
        /// </summary>
        public int[] BoostedVersions { get; set; } = [];
    }

    public sealed class AbilityGemsRowMap : ClassMap<AbilityGemsRow>
    {
        public AbilityGemsRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Gems).Index(2);
            Map(m => m.BoostedVersions).Index(3).TypeConverter<IntArrayConverter>();
        }
    }
}