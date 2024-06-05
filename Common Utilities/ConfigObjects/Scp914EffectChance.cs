namespace Common_Utilities.ConfigObjects
{
    using Exiled.API.Enums;

    public class Scp914EffectChance : IChanceObject
    {
        public EffectType Effect { get; set; }

        public double Chance { get; set; }

        public float Duration { get; set; }

        public void Deconstruct(out EffectType effect, out double chance, out float duration)
        {
            effect = Effect;
            chance = Chance;
            duration = Duration;
        }
    }
}