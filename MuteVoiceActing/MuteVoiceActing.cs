using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;
using Dust.Audio;
using Microsoft.Xna.Framework.Audio;

namespace MuteVoiceActing
{
    public class MuteVoiceActing : ILoader
    {
        static AudioEngine engine;

        public string Name
        {
            get { return "Voice Acting Muter"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Type myType = GetType();
            MethodInfo setSfxVolumeMethod = typeof(VoiceCue).Assembly.GetType("Dust.Audio.Sound").GetMethod("SetSFXVolume");
            MethodUtil.ReplaceMethod(setSfxVolumeMethod, myType.GetMethod("OrigSetSFXVolume"));
            MethodUtil.ReplaceMethod(myType.GetMethod("SetSFXVolumeReplacement"), setSfxVolumeMethod);
        }

        public static void SetSFXVolumeReplacement(float _sfxVolume)
        {
            if (engine == null) engine = (AudioEngine)typeof(VoiceCue).Assembly.GetType("Dust.Audio.Sound").GetField("engine").GetValue(null);
            OrigSetSFXVolume(_sfxVolume);
            engine.GetCategory("Voice").SetVolume(0);
        }

        public static void OrigSetSFXVolume(float _sfxVolume)
        {
        }
    }
}
