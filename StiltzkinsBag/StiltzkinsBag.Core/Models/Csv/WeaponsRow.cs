using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from Weapons.csv.
    /// Columns: Comment;Id;Category;StatusIndex;Model;ScriptId;Power;Elements;Rate;Offset1;Offset2;HitSfx;CustomTexture
    /// </summary>
    public sealed class WeaponsRow
    {
        /// <summary>Weapon name label (e.g. "Dagger").</summary>
        public string Comment { get; set; } = string.Empty;

        public int Id { get; set; }
        public byte Category { get; set; }
        public int StatusIndex { get; set; }

        /// <summary>Model identifier string (e.g. "GEO_WEP_B1_012").</summary>
        public string Model { get; set; } = string.Empty;

        public int ScriptId { get; set; }
        public int Power { get; set; }
        public byte Elements { get; set; }
        public int Rate { get; set; }
        public short Offset1 { get; set; }
        public short Offset2 { get; set; }
        public byte HitSfx { get; set; }

        /// <summary>Optional custom texture name(s). Empty array when column is blank.</summary>
        public string[] CustomTexture { get; set; } = [];
    }

    public sealed class WeaponsRowMap : ClassMap<WeaponsRow>
    {
        public WeaponsRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Category).Index(2);
            Map(m => m.StatusIndex).Index(3);
            Map(m => m.Model).Index(4);
            Map(m => m.ScriptId).Index(5);
            Map(m => m.Power).Index(6);
            Map(m => m.Elements).Index(7);
            Map(m => m.Rate).Index(8);
            Map(m => m.Offset1).Index(9);
            Map(m => m.Offset2).Index(10);
            Map(m => m.HitSfx).Index(11);
            Map(m => m.CustomTexture).Index(12).TypeConverter<StringArrayConverter>();
        }
    }
}