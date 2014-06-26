using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using DustAetPatchingPlatform;
using Dust;
using Dust.PCClasses;

namespace PrevStateFix
{
    class StateFixer : ILoader
    {
        public string Name
        {
            get { return "prev*State Fixer"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Platform.RegisterLoadAfterGameInit(fix);
        }

        void fix()
        {
            MouseState dummyMouseState = new MouseState();
            AlternateKeyboardLayouts dummyKeyboardLayouts = new AlternateKeyboardLayouts(new KeyboardState());

            Type pcManagerType = typeof(PCManager);
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
            pcManagerType.GetField("prevHudKeyboardState", bf).SetValue(Game1.pcManager, dummyKeyboardLayouts);
            pcManagerType.GetField("prevHudMouseState", bf).SetValue(Game1.pcManager, dummyMouseState);
            pcManagerType.GetField("prevPlayerKeyboardState", bf).SetValue(Game1.pcManager, dummyKeyboardLayouts);
            pcManagerType.GetField("prevPlayerMouseState", bf).SetValue(Game1.pcManager, dummyMouseState);
            pcManagerType.GetField("prevShopKeyboardState", bf).SetValue(Game1.pcManager, dummyKeyboardLayouts);
            pcManagerType.GetField("prevShopMouseState", bf).SetValue(Game1.pcManager, dummyMouseState);
            pcManagerType.GetField("prevWorldMapKeyboardState", bf).SetValue(Game1.pcManager, dummyKeyboardLayouts);
            pcManagerType.GetField("prevWorldMapMouseState", bf).SetValue(Game1.pcManager, dummyMouseState);
        }
    }
}
