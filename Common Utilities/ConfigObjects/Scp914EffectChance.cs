namespace Common_Utilities.ConfigObjects
{
    using System.ComponentModel;

    using Exiled.API.Enums;

    /// <summary>
    /// Represents the chance of an SCP-914 effect occurring.
    /// </summary>
    public class Scp914EffectChance
    {
        /// <summary>
        /// Gets or sets the type of effect.
        /// </summary>
        [Description("The type of SCP-914 effect.")]
        public EffectType Effect { get; set; }

        /// <summary>
        /// Gets or sets the chance of the effect occurring.
        /// </summary>
        [Description("The probability of the SCP-914 effect occurring.")]
        public double Chance { get; set; }

        /// <summary>
        /// Gets or sets the duration of the effect.
        /// </summary>
        [Description("The duration of the SCP-914 effect.")]
        public float Duration { get; set; }

        /// <summary>
        /// Deconstructs the object into its effect type, chance, and duration.
        /// </summary>
        /// <param name="effect">The type of SCP-914 effect.</param>
        /// <param name="chance">The probability of the effect occurring.</param>
        /// <param name="duration">The duration of the effect.</param>
        public void Deconstruct(out EffectType effect, out double chance, out float duration)
        {
            effect = Effect;
            chance = Chance;
            duration = Duration;
        }
    }
}