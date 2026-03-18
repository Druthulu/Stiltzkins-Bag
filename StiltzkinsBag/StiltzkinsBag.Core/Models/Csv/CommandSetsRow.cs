using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from CommandSets.csv.
    /// Columns: Id;Attack;Defend;Regular1;Regular2;Item;Change;AttackTrance;DefendTrance;Trance1;Trance2;ItemTrance;ChangeTrance
    ///
    /// Note: the source file uses heavy tab padding between fields.
    /// CsvParser's TrimOptions.Trim handles this — values arrive clean.
    /// </summary>
    public sealed class CommandSetsRow
    {
        public int Id { get; set; }
        public int Attack { get; set; }
        public int Defend { get; set; }

        /// <summary>First regular (non-trance) command slot.</summary>
        public int Regular1 { get; set; }

        /// <summary>Second regular (non-trance) command slot.</summary>
        public int Regular2 { get; set; }

        public int Item { get; set; }
        public int Change { get; set; }
        public int AttackTrance { get; set; }
        public int DefendTrance { get; set; }

        /// <summary>First trance command slot.</summary>
        public int Trance1 { get; set; }

        /// <summary>Second trance command slot.</summary>
        public int Trance2 { get; set; }

        public int ItemTrance { get; set; }
        public int ChangeTrance { get; set; }
    }

    public sealed class CommandSetsRowMap : ClassMap<CommandSetsRow>
    {
        public CommandSetsRowMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.Attack).Index(1);
            Map(m => m.Defend).Index(2);
            Map(m => m.Regular1).Index(3);
            Map(m => m.Regular2).Index(4);
            Map(m => m.Item).Index(5);
            Map(m => m.Change).Index(6);
            Map(m => m.AttackTrance).Index(7);
            Map(m => m.DefendTrance).Index(8);
            Map(m => m.Trance1).Index(9);
            Map(m => m.Trance2).Index(10);
            Map(m => m.ItemTrance).Index(11);
            Map(m => m.ChangeTrance).Index(12);
        }
    }
}