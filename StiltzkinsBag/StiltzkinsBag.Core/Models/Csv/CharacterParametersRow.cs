using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from CharacterParameters.csv.
    /// Columns: Id;DefaultRow;DefaultWinPose;DefaultCategory;DefaultCommandSet;DefaultEquipmentSet;BattleParameterFormula;NameKeyword
    /// </summary>
    public sealed class CharacterParametersRow
    {
        public int Id { get; set; }
        public bool DefaultRow { get; set; }
        public bool DefaultWinPose { get; set; }
        public byte DefaultCategory { get; set; }
        public int DefaultCommandSet { get; set; }
        public int DefaultEquipmentSet { get; set; }

        /// <summary>
        /// NCalc formula string for battle parameter selection.
        /// May be a plain integer or a conditional expression.
        /// Preserved verbatim — never evaluated by the randomizer.
        /// </summary>
        public string BattleParameterFormula { get; set; } = string.Empty;

        /// <summary>Four-character name keyword used by Memoria (e.g. "ZDNE").</summary>
        public string NameKeyword { get; set; } = string.Empty;
    }

    public sealed class CharacterParametersRowMap : ClassMap<CharacterParametersRow>
    {
        public CharacterParametersRowMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.DefaultRow).Index(1).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.DefaultWinPose).Index(2).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.DefaultCategory).Index(3);
            Map(m => m.DefaultCommandSet).Index(4);
            Map(m => m.DefaultEquipmentSet).Index(5);
            Map(m => m.BattleParameterFormula).Index(6);
            Map(m => m.NameKeyword).Index(7);
        }
    }
}