using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustAetPatchingPlatform
{
    /// <summary>
    /// Interface for loading patches.
    /// </summary>
    public interface ILoader
    {
        /// <summary>
        /// Gets the name of the patch.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the priority of the patch.
        /// </summary>
        /// <remarks>Default priority is 1000; the smaller the number, the higher the priority.</remarks>
        int Priority { get; } 
        /// <summary>
        /// Loads the patch.
        /// </summary>
        void Load();
    }
}
