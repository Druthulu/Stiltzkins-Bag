using CsvHelper.Configuration;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Models.Csv
{
    /// <summary>
    /// One row from Synthesis.csv.
    /// Columns: Comment;Id;Shops;Price;Result;Ingredients
    ///
    /// Shops and Ingredients are comma-separated int[] fields.
    /// Result is the output item ID.
    /// </summary>
    public sealed class SynthesisRow
    {
        /// <summary>Recipe name label (e.g. "Butterfly Sword").</summary>
        public string Comment { get; set; } = string.Empty;

        public int Id { get; set; }

        /// <summary>Synthesis shop IDs where this recipe is available.</summary>
        public int[] Shops { get; set; } = [];

        public uint Price { get; set; }

        /// <summary>Item ID produced by this recipe.</summary>
        public int Result { get; set; }

        /// <summary>Item IDs required as ingredients.</summary>
        public int[] Ingredients { get; set; } = [];
    }

    public sealed class SynthesisRowMap : ClassMap<SynthesisRow>
    {
        public SynthesisRowMap()
        {
            Map(m => m.Comment).Index(0);
            Map(m => m.Id).Index(1);
            Map(m => m.Shops).Index(2).TypeConverter<IntArrayConverter>();
            Map(m => m.Price).Index(3);
            Map(m => m.Result).Index(4);
            Map(m => m.Ingredients).Index(5).TypeConverter<IntArrayConverter>();
        }
    }
}