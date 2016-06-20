using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DustAetPatchingPlatform;

namespace PartsWrapped
{
    /// <summary>
    /// External class used by Dust: AET.
    /// </summary>
    public static class PartsWraps
    {
        /// <summary>
        /// Method called by Dust: AET to apply this module.
        /// </summary>
        public static void chk()
        {
            // Insert hooking code here...
            ExceptionCatcher.RegisterCatcher();
            Platform.InitPlatform();
        }
    }
}
