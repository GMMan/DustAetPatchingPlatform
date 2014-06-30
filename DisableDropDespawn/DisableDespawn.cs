using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;
using Dust.Particles;

namespace DisableDropDespawn
{
    class DisableDespawn : ILoader
    {
        static readonly Type ParticleType = typeof(Particle);
        static readonly Type CoinType = ParticleType.Assembly.GetType("Dust.Particles.Coin");
        static readonly FieldInfo CoinFrameField = CoinType.GetField("frame", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly Type EquipmentType = ParticleType.Assembly.GetType("Dust.Particles.Equipment");
        static readonly FieldInfo EquipmentFrameField = EquipmentType.GetField("frame", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly Type MaterialType = ParticleType.Assembly.GetType("Dust.Particles.Material");
        static readonly FieldInfo MaterialFrameField = MaterialType.GetField("frame", BindingFlags.Instance | BindingFlags.NonPublic);

        public string Name
        {
            get { return "Disable Drop Despawn Mod"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Type myType = GetType();
            MethodUtil.ReplaceMethod(ParticleType.GetMethod("Reset"), myType.GetMethod("OrigParticleReset"));
            MethodUtil.ReplaceMethod(myType.GetMethod("ParticleResetReplacement"), ParticleType.GetMethod("Reset"));
        }

        public void ParticleResetReplacement()
        {
            switch (this.GetType().Name)
            {
                // Note: taking advantage of *.frame most likely not being exactly 0 when Reset() is called to determine if we're in game or in menu.
                case "Coin":
                    if ((float)CoinFrameField.GetValue(this) < 0)
                    {
                        CoinFrameField.SetValue(this, 12);
                        return;
                    }
                    break;
                case "Equipment":
                    if ((float)EquipmentFrameField.GetValue(this) < 0)
                    {
                        EquipmentFrameField.SetValue(this, 20);
                        return;
                    }
                    break;
                case "Material":
                    if ((float)MaterialFrameField.GetValue(this) < 0)
                    {
                        MaterialFrameField.SetValue(this, 20);
                        return;
                    }
                    break;
            }

            OrigParticleReset();
        }

        public void OrigParticleReset()
        {
            // Stub for original Particle.Reset() method
        }
    }
}
