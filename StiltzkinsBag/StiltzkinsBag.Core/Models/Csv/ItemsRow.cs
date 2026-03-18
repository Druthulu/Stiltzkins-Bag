using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from Items.csv.
    /// Columns: Id;WeaponId;ArmorId;EffectId;Price;SellingPrice;GraphicsId;ColorId;Quality;BonusId;
    ///          AbilityIds;Weapon;Armlet;Helmet;Armor;Accessory;Item;Gem;Usable;Order;
    ///          Zidane;Vivi;Garnet;Steiner;Freya;Quina;Eiko;Amarant;Cinna;Marcus;Blank;Beatrix
    ///
    /// AbilityIds is a mixed ability ref array: "AA:101, SA:3, AA:102"
    /// Equip flag columns (Weapon…Usable) and per-character equip columns are stored as bool (0/1).
    /// Quality and Order are stored as float in the source.
    /// </summary>
    public sealed class ItemsRow
    {
        public int Id { get; set; }
        public int WeaponId { get; set; }
        public int ArmorId { get; set; }
        public int EffectId { get; set; }
        public uint Price { get; set; }
        public int SellingPrice { get; set; }
        public byte GraphicsId { get; set; }
        public byte ColorId { get; set; }
        public float Quality { get; set; }

        /// <summary>Index into Stats.csv for this item's stat bonuses.</summary>
        public int BonusId { get; set; }

        /// <summary>Comma-separated ability references, e.g. "AA:101, SA:3". May be empty.</summary>
        public string[] AbilityIds { get; set; } = [];

        // --- Equipment type flags ---
        public bool Weapon { get; set; }
        public bool Armlet { get; set; }
        public bool Helmet { get; set; }
        public bool Armor { get; set; }
        public bool Accessory { get; set; }
        public bool Item { get; set; }
        public bool Gem { get; set; }
        public bool Usable { get; set; }

        /// <summary>Sort order value (float).</summary>
        public float Order { get; set; }

        // --- Per-character equip flags ---
        public bool Zidane { get; set; }
        public bool Vivi { get; set; }
        public bool Garnet { get; set; }
        public bool Steiner { get; set; }
        public bool Freya { get; set; }
        public bool Quina { get; set; }
        public bool Eiko { get; set; }
        public bool Amarant { get; set; }
        public bool Cinna { get; set; }
        public bool Marcus { get; set; }
        public bool Blank { get; set; }
        public bool Beatrix { get; set; }
    }

    public sealed class ItemsRowMap : ClassMap<ItemsRow>
    {
        public ItemsRowMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.WeaponId).Index(1);
            Map(m => m.ArmorId).Index(2);
            Map(m => m.EffectId).Index(3);
            Map(m => m.Price).Index(4);
            Map(m => m.SellingPrice).Index(5);
            Map(m => m.GraphicsId).Index(6);
            Map(m => m.ColorId).Index(7);
            Map(m => m.Quality).Index(8);
            Map(m => m.BonusId).Index(9);
            Map(m => m.AbilityIds).Index(10).TypeConverter<AbilityRefArrayConverter>();
            Map(m => m.Weapon).Index(11).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Armlet).Index(12).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Helmet).Index(13).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Armor).Index(14).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Accessory).Index(15).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Item).Index(16).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Gem).Index(17).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Usable).Index(18).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Order).Index(19);
            Map(m => m.Zidane).Index(20).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Vivi).Index(21).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Garnet).Index(22).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Steiner).Index(23).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Freya).Index(24).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Quina).Index(25).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Eiko).Index(26).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Amarant).Index(27).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Cinna).Index(28).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Marcus).Index(29).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Blank).Index(30).TypeConverter<MemoriaBoolConverter>();
            Map(m => m.Beatrix).Index(31).TypeConverter<MemoriaBoolConverter>();
        }
    }
}