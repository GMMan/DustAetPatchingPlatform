using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;*/
using Microsoft.Xna.Framework.Graphics;
/*using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;*/

namespace fbDeprofiler
{
    internal class GraphicsAdapterReplacement
    {
        public bool IsProfileSupported(GraphicsProfile profile)
        {
            return true;
        }

        //[return: MarshalAs(UnmanagedType.U1)]
        internal bool IsProfileSupported(Enum deviceType, GraphicsProfile graphicsProfile)
        {
            return true;
        }
    }
}
