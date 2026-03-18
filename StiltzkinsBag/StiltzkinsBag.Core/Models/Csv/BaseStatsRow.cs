using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from BaseStats.csv.
    /// Columns: Comment;Id;Dexterity;Strength;Magic;Will;Gems
    /// </summary>
    public sealed class BaseStatsRow
    {
        /// <summary>Character name label (e.g. "Zidane"). Not written back — comment column.</summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>Character index (0 = Zidane … 11 = Beatrix).</summary>
        public int Id { get; set; }

        public byte Dexterity { get; set; }
        public byte Strength { get; set; }
        public byte Magic { get; set; }
        public byte Will { get; set; }

        /// <summary>Gem budget — number of ability gem slots available.</summary>
        public uint Gems { get; set; }
    }

    public sealed class BaseStatsRowMap : ClassMap<BaseStatsRow>
    {
        public BaseStatsRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Dexterity).Index(2);
            Map(m => m.Strength).Index(3);
            Map(m => m.Magic).Index(4);
            Map(m => m.Will).Index(5);
            Map(m => m.Gems).Index(6);
        }
    }
}