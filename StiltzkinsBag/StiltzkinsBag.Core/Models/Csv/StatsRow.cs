using CsvHelper.Configuration;

namespace StiltzkinsBag.Models.Csv;

/// <summary>
/// One row from Stats.csv — a named bonus set assigned to a piece of equipment
/// via its <c>BonusId</c> column in Items.csv, Weapons.csv, or Armors.csv.
///
/// Columns: Comment;Id;Dexterity;Strength;Magic;Will;
///          AttackElement;GuardElement;AbsorbElement;HalfElement;WeakElement
///
/// The four stat columns (Dexterity–Will) are the only fields modified by
/// <c>GearStatRandomizer</c>. Element columns are read and preserved verbatim.
/// </summary>
public class StatsRow
{
    /// <summary>Human-readable name, e.g. "Bonus 0045 # Thief Hat".</summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>Row ID referenced by BonusId in equipment CSV files.</summary>
    public int Id { get; set; }

    // -------------------------------------------------------------------------
    // Stat bonuses (randomizable)
    // -------------------------------------------------------------------------

    public byte Dexterity { get; set; }
    public byte Strength { get; set; }
    public byte Magic { get; set; }
    public byte Will { get; set; }

    // -------------------------------------------------------------------------
    // Elemental flags (never modified by GearStatRandomizer)
    // -------------------------------------------------------------------------

    /// <summary>Elemental attack flag bitmask.</summary>
    public byte AttackElement { get; set; }

    /// <summary>Elemental guard (resistance) flag bitmask.</summary>
    public byte GuardElement { get; set; }

    /// <summary>Elemental absorb flag bitmask.</summary>
    public byte AbsorbElement { get; set; }

    /// <summary>Elemental half-damage flag bitmask.</summary>
    public byte HalfElement { get; set; }

    /// <summary>Elemental weakness flag bitmask.</summary>
    public byte WeakElement { get; set; }

    /// <summary>
    /// Returns true if any stat column has a non-zero vanilla value.
    /// Used by GearStatRandomizer to identify stat-bearing rows.
    /// </summary>
    public bool HasAnyStat => Dexterity > 0 || Strength > 0 || Magic > 0 || Will > 0;
}

/// <summary>
/// CsvHelper ClassMap for <see cref="StatsRow"/>. Positional mapping matches
/// Memoria's semicolon-delimited Stats.csv column order exactly.
/// </summary>
public sealed class StatsRowMap : ClassMap<StatsRow>
{
    public StatsRowMap()
    {
        Map(r => r.Comment).Index(0);
        Map(r => r.Id).Index(1);
        Map(r => r.Dexterity).Index(2);
        Map(r => r.Strength).Index(3);
        Map(r => r.Magic).Index(4);
        Map(r => r.Will).Index(5);
        Map(r => r.AttackElement).Index(6);
        Map(r => r.GuardElement).Index(7);
        Map(r => r.AbsorbElement).Index(8);
        Map(r => r.HalfElement).Index(9);
        Map(r => r.WeakElement).Index(10);
    }
}