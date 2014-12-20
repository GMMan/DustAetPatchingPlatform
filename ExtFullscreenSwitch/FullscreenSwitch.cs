using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using DustAetPatchingPlatform;
using Microsoft.Xna.Framework;
using Dust;

namespace ExtFullscreenSwitch
{
    public class FullscreenSwitch : ILoader
    {
        bool ranOnce = false;
        bool isFullscreen;

        public string Name
        {
            get { return "Command Line Full Screen Switch Implementation"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Platform.RegisterLoadAfterGameInit(parseAndSetFullscreen);
        }

        void parseAndSetFullscreen()
        {
            bool switched = false;

            if (!ranOnce)
            {
                while (Game1.storage == null || Game1.storage.storeResult != Dust.Storage.StoreResult.Loaded) Thread.Sleep(50);

                string[] args = Environment.GetCommandLineArgs();
                for (int i = 1; i < args.Length && !switched; ++i)
                {
                    string arg = args[i].ToUpperInvariant();
                    if (arg == "/WINDOW")
                    {
                        isFullscreen = false;
                        switched = true;
                    }
                    else if (arg == "/FULLSCREEN")
                    {
                        isFullscreen = true;
                        switched = true;
                    }
                }
            }
            else
            {
                while (Game1.menu.curMenuPage < 0) Thread.Sleep(50);
            }

            if (switched || ranOnce)
            {
                Game1.settings.FullScreen = Game1.graphics.IsFullScreen = isFullscreen;
                Application.OpenForms[0].Invoke(new Action(Game1.graphics.ApplyChanges));

                // Pressing any key reloads settings for some stupid reason.
                // They're global. Once you've loaded it, you don't need to do it again.
                // It's not like it's going to change externally.
                if (!ranOnce)
                {
                    ranOnce = true;
                    new Thread(parseAndSetFullscreen).Start();
                }
            }
        }
    }
}
