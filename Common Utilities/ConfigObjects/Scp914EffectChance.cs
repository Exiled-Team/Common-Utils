namespace Common_Utilities.ConfigObjects
{
    using Exiled.API.Enums;

    public class Scp914EffectChance
    {
        public EffectType Effect { get; set; }
        public int Chance { get; set; }
        public float Duration { get; set; }

        public void Deconstruct(out EffectType effect, out int chance, out float duration)
        {
            effect = Effect;
            chance = Chance;
            duration = Duration;
        }
    }
}