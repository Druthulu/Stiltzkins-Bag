using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from Armors.csv.
    /// Columns: Comment;Id;P.Def;P.Eva;M.Def;M.Eva
    ///
    /// Covers all armor slot types: wrists, gloves, helmets, body armor, accessories with def values.
    /// </summary>
    public sealed class ArmorsRow
    {
        /// <summary>Armor name label (e.g. "Wrist", "Leather Wrist").</summary>
        public string Comment { get; set; } = string.Empty;

        public int Id { get; set; }
        public int PDef { get; set; }
        public int PEva { get; set; }
        public int MDef { get; set; }
        public int MEva { get; set; }
    }

    public sealed class ArmorsRowMap : ClassMap<ArmorsRow>
    {
        public ArmorsRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.PDef).Index(2);
            Map(m => m.PEva).Index(3);
            Map(m => m.MDef).Index(4);
            Map(m => m.MEva).Index(5);
        }
    }
}