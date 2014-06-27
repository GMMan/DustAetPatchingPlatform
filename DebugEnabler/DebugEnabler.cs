using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DustAetPatchingPlatform;
using Dust;

namespace DebugEnabler
{
    class DebugEnabler : ILoader
    {
        public string Name
        {
            get { return "Debug Enabler"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            Game1.canDebug = true;
        }
    }
}
