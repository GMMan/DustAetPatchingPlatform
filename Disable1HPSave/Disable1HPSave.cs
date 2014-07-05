using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;
using Dust;
using Dust.Particles;
using Dust.CharClasses;

namespace Disable1HPSave
{
    class Disable1HPSave : ILoader
    {
        public string Name
        {
            get { return "Disable 1 HP Save Mod"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Type myType = GetType();
            MethodInfo updateStatsMethod = typeof(Stats).Assembly.GetType("Dust.HitManager").GetMethod("UpdateStats", BindingFlags.Static | BindingFlags.NonPublic);
            MethodUtil.ReplaceMethod(updateStatsMethod, myType.GetMethod("OrigUpdateStats", BindingFlags.Static | BindingFlags.NonPublic));
            MethodUtil.ReplaceMethod(myType.GetMethod("UpdateStatsReplacement", BindingFlags.Static | BindingFlags.NonPublic), updateStatsMethod);
        }

        static void UpdateStatsReplacement(Character[] c, ParticleManager pMan, StatusEffects sType, int i, int pOwner, float finalHitValue, float recharge)
        {
            OrigUpdateStats(c, pMan, sType, i, pOwner, finalHitValue, recharge);
            if (i == 0)
            {
                // finalHitValue gets adjusted in numerous places in the original code, and I'm too lazy and don't want to reimplement that code,
                // so I will assume if you only have 1 HP you would have been dead anyway. It's not your lucky day.
                // while loop just in case UpdateStats() adjusts the hit to be > 1 (or < 1, which casts to 0) and nullify it.
                while (c[i].HP == 1)
                {
                    OrigUpdateStats(c, pMan, sType, i, pOwner, 1f, recharge);
                }
            }
        }

        static void OrigUpdateStats(Character[] c, ParticleManager pMan, StatusEffects sType, int i, int pOwner, float finalHitValue, float recharge)
        {
            // Stub to be replaced with original HitManager.UpdateStats() method.
        }
    }
}
