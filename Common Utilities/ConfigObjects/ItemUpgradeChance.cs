namespace Common_Utilities.ConfigObjects
{
    using System.ComponentModel;

    /// <summary>
    /// Represents the chance of upgrading an item to a new item.
    /// </summary>
    public class ItemUpgradeChance
    {
        /// <summary>
        /// Gets or sets the original item before upgrade.
        /// </summary>
        [Description("The original item before the upgrade.")]
        public object OriginalItem { get; set; }

        /// <summary>
        /// Gets or sets the new item after the upgrade.
        /// </summary>
        [Description("The new item after the upgrade.")]
        public object NewItem { get; set; }

        /// <summary>
        /// Gets or sets the chance of upgrading the item.
        /// </summary>
        [Description("The probability of upgrading the original item to the new item.")]
        public double Chance { get; set; }

        /// <summary>
        /// Gets or sets the number of items involved in the upgrade.
        /// Defaults to 1.
        /// </summary>
        [Description("The quantity of items involved in the upgrade process.")]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Deconstructs the object into its original item, upgrade chance, and count.
        /// </summary>
        /// <param name="original">The original item.</param>
        /// <param name="i">The upgrade chance.</param>
        /// <param name="count">The quantity of items.</param>
        public void Deconstruct(out object original, out double i, out int count)
        {
            Deconstruct(out original, out _, out i, out count);
        }

        /// <summary>
        /// Deconstructs the object into its original item, new item, upgrade chance, and count.
        /// </summary>
        /// <param name="original">The original item.</param>
        /// <param name="newItem">The new item after the upgrade.</param>
        /// <param name="i">The upgrade chance.</param>
        /// <param name="count">The quantity of items.</param>
        public void Deconstruct(out object original, out object newItem, out double i, out int count)
        {
            original = OriginalItem;
            newItem = NewItem;
            i = Chance;
            count = Count;
        }
    }
}