using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from any per-character ability CSV
    /// (Zidane.csv, Vivi.csv, Garnet.csv, Steiner.csv, Freya.csv, Quina.csv,
    ///  Eiko.csv, Amarant.csv, Cinna1.csv, Cinna2.csv, Marcus1.csv, Marcus2.csv,
    ///  Blank1.csv, Blank2.csv, Beatrix1.csv, Beatrix2.csv).
    ///
    /// Columns: Id;AP
    ///
    /// Id is an ability reference string: "AA:101" (active ability) or "SA:3" (support ability).
    /// AP is the ability point cost to learn the ability via equipment.
    /// </summary>
    public sealed class CharacterAbilityRow
    {
        /// <summary>
        /// Ability reference, e.g. "AA:101" or "SA:3".
        /// Preserved verbatim — never parsed into a numeric ID by this layer.
        /// </summary>
        public string AbilityRef { get; set; } = string.Empty;

        /// <summary>AP cost to master this ability.</summary>
        public int AP { get; set; }
    }

    public sealed class CharacterAbilityRowMap : ClassMap<CharacterAbilityRow>
    {
        public CharacterAbilityRowMap()
        {
            Map(m => m.AbilityRef).Index(0);
            Map(m => m.AP).Index(1);
        }
    }
}