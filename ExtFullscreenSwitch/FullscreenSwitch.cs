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
            if (!ranOnce)
            {
                while (Game1.storage == null || Game1.storage.storeResult != Dust.Storage.StoreResult.Loaded) Thread.Sleep(50);
            }
            else
            {
                while (Game1.menu.curMenuPage < 0) Thread.Sleep(50);
            }

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; ++i)
            {
                string arg = args[i].ToUpperInvariant();
                bool switched = false;
                if (arg == "/WINDOW")
                {
                    Game1.settings.FullScreen = Game1.graphics.IsFullScreen = false;
                    switched = true;
                }
                else if (arg == "/FULLSCREEN")
                {
                    Game1.settings.FullScreen = Game1.graphics.IsFullScreen = true;
                    switched = true;
                }

                if (switched)
                {
                    // Yank control out of Game1 instance until I can figure out how to find the primary thread
                    GameWindow window = ((Game)typeof(GraphicsDeviceManager).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Game1.graphics)).Window;
                    Form form = (Form)window.GetType().GetProperty("Form", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(window, null);
                    form.Invoke(new Action(Game1.graphics.ApplyChanges));
                    break;
                }
            }

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
