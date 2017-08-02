using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;
using Dust;
using Dust.CharClasses;

namespace OverheatDisabler
{
    public class DisableOverheat : ILoader
    {
        public string Name => "Dust Storm Overheat Disabler";

        public int Priority => 1000;

        public void Load()
        {
            Type myType = GetType();
            MethodInfo spinBladeMethod = typeof(Character).GetMethod("SpinBlade", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodUtil.ReplaceMethod(spinBladeMethod, myType.GetMethod(nameof(OrigSpinBlade), BindingFlags.Instance | BindingFlags.NonPublic));
            MethodUtil.ReplaceMethod(myType.GetMethod(nameof(SpinBladeReplacement), BindingFlags.Instance | BindingFlags.NonPublic), spinBladeMethod);
        }

        void SpinBladeReplacement(Character[] c)
        {
            if (Game1.stats.isSpinning && Game1.stats.overHeating + Game1.FrameTime > 5)
            {
                Game1.stats.overHeating = 5 - Game1.FrameTime;
            }
            OrigSpinBlade(c);
        }

        void OrigSpinBlade(Character[] c)
        { }
    }
}
