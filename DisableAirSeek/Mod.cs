using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;
using Dust.CharClasses;

namespace DisableAirSeek
{
    class Mod : ILoader
    {
        public string Name
        {
            get { return "Disable Aerial Dust Storm Seeking Mod"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            MethodUtil.ReplaceMethod(typeof(Mod).GetMethod("SeekBoundsReplacement"), typeof(Character).GetMethod("SeekBounds", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        public bool SeekBoundsReplacement(Character[] c, int i)
        {
            return false;
        }
    }
}
