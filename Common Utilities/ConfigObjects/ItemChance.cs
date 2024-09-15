namespace Common_Utilities.ConfigObjects;

using System.ComponentModel;

/// <summary>
/// Represents an item with a corresponding chance and group.
/// </summary>
public class ItemChance
{
    /// <summary>
    /// Gets or sets the name of the item.
    /// Defaults to <see cref="ItemType.None"/>.
    /// </summary>
    [Description("The name of the item.")]
    public string Item { get; set; } = ItemType.None.ToString();

    /// <summary>
    /// Gets or sets the chance of this item appearing.
    /// A double value representing the likelihood.
    /// </summary>
    [Description("The chance for the item to appear, as a double value.")]
    public double Chance { get; set; }

    /// <summary>
    /// Gets or sets the group to which the item belongs.
    /// Defaults to 'none'.
    /// </summary>
    [Description("The group to which this item belongs.")]
    public string Group { get; set; } = "none";

    /// <summary>
    /// Deconstructs the <see cref="ItemChance"/> object into individual properties.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="i">The chance of the item appearing.</param>
    /// <param name="groupKey">The group to which the item belongs.</param>
    public void Deconstruct(out string name, out double i, out string groupKey)
    {
        name = Item;
        i = Chance;
        groupKey = Group;
    }
}