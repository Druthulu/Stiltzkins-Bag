using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from DefaultEquipment.csv.
    /// Columns: Comment;Id;Weapon;Head;Wrist;Armor;Accessory
    ///
    /// Slot values are item IDs. -1 means the slot is empty.
    /// </summary>
    public sealed class DefaultEquipmentRow
    {
        /// <summary>Character or set label (e.g. "Zidane", "Marcus 2").</summary>
        public string Comment { get; set; } = string.Empty;

        public int Id { get; set; }
        public int Weapon { get; set; }
        public int Head { get; set; }
        public int Wrist { get; set; }
        public int Armor { get; set; }
        public int Accessory { get; set; }
    }

    public sealed class DefaultEquipmentRowMap : ClassMap<DefaultEquipmentRow>
    {
        public DefaultEquipmentRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Weapon).Index(2);
            Map(m => m.Head).Index(3);
            Map(m => m.Wrist).Index(4);
            Map(m => m.Armor).Index(5);
            Map(m => m.Accessory).Index(6);
        }
    }
}