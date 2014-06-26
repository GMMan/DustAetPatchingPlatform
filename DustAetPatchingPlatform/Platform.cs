using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Dust;

namespace DustAetPatchingPlatform
{
    /// <summary>
    /// Provides methods for use with the patching platform.
    /// </summary>
    public static class Platform
    {
        static List<Action> PostInitLoaders { get; set; }
        static Thread GameInitDetectorThread;

        /// <summary>
        /// Gets a value that indicates if the platform has been initialized.
        /// </summary>
        public static bool IsInit { get; private set; }

        /// <summary>
        /// Initializes the platform.
        /// </summary>
        internal static void InitPlatform()
        {
            // Static property initialization
            PostInitLoaders = new List<Action>();

            // Load patches
            ModuleLoader.LoadAllFromPluginsDir();
            checkLoadErrors();

            // Start delayed loading thread if any patches request so.
            if (PostInitLoaders.Count > 0)
            {
                GameInitDetectorThread = new Thread(loadAfterInitProc);
                GameInitDetectorThread.Start();
            }

            IsInit = true;
        }

        /// <summary>
        /// Checks and displays errors during patch loading.
        /// </summary>
        static void checkLoadErrors()
        {
            if (ModuleLoader.LoadErrors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("There were errors while loading patches. Here's a summary: ");
                sb.AppendLine();
                foreach (ModuleLoader.LoadingException loadError in ModuleLoader.LoadErrors)
                {
                    sb.AppendLine(loadError.Message);
                }
                sb.AppendLine();
                sb.AppendLine("Please check if the patches' dependencies are satisfied. Click Yes to copy the complete error report to clipboard, or no to continue.");

                if (MessageBox.Show(sb.ToString(), "Error during patch loading", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    sb.Clear();
                    foreach (ModuleLoader.LoadingException loadError in ModuleLoader.LoadErrors)
                    {
                        sb.Append("Path/Name: ");
                        sb.AppendLine(loadError.ModulePath);
                        sb.Append("Exception: ");
                        sb.AppendLine(loadError.InnerException.ToString());
                        sb.AppendLine();
                    }

                    ExceptionCatcher.RunAsSTAThread(() => Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText));
                    MessageBox.Show("The extended information has been placed on the clipboard. Please submit it to the Dust: AET Patching Platform thread on the game's Steam Community forum.", "Error during patch loading", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            ModuleLoader.LoadErrors.Clear();
        }

        /// <summary>
        /// Registers a patch loading delegate to be called after the game has initialized.
        /// </summary>
        /// <param name="loadDelegate">The delegate to be called.</param>
        public static void RegisterLoadAfterGameInit(Action loadDelegate)
        {
            if (PostInitLoaders == null) throw new InvalidOperationException("Platform has not been initialized.");

            PostInitLoaders.Add(loadDelegate);
        }

        static void loadAfterInitProc()
        {
            while (Game1.pcManager == null) ;// Thread.Sleep(100);
            foreach (Action loadDelegate in PostInitLoaders)
            {
                try
                {
                    loadDelegate();
                }
                catch (Exception ex)
                {
                    ModuleLoader.LoadErrors.Add(new ModuleLoader.LoadingException(loadDelegate.Method.ReflectedType.FullName + "." + loadDelegate.Method.Name, ex));
                }
            }
            checkLoadErrors();
        }

        /// <summary>
        /// Determine whether or not a type is a concrete implementation of the current type.
        /// </summary>
        /// <param name="iface">The type that can be implemented</param>
        /// <param name="type">The type that is being checked against</param>
        /// <returns></returns>
        public static bool IsConcretelyImplementedBy(this Type iface, Type type)
        {
            return iface.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
        }
    }
}
