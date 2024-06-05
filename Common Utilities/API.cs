namespace Common_Utilities;

using System.Collections.Generic;
using System.Linq;
using ConfigObjects;

public class API
{
    public static double RollChance(IEnumerable<IChanceObject> scp914EffectChances)
    {
        double rolledChance;
        
        if (Plugin.Instance.Config.AdditiveProbabilities)
            rolledChance = Plugin.Random.NextDouble() * scp914EffectChances.Sum(x => x.Chance);
        else
            rolledChance = Plugin.Random.NextDouble() * 100;
        
        return rolledChance;
    }
}