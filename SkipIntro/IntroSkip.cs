using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DustAetPatchingPlatform;
using Dust;

namespace SkipIntro
{
    class IntroSkip : ILoader
    {
        public string Name
        {
            get { return "Intro Skipper"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Platform.RegisterLoadAfterGameInit(doLoad);
        }

        void doLoad()
        {
            Game1.gameMode = Game1.GameModes.MainMenu;
        }
    }
}
