using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Threading;

namespace DustAetPatchingPlatform
{
    /// <summary>
    /// Class for catching uncaught exceptions and to present error information.
    /// </summary>
    public static class ExceptionCatcher
    {
        /// <summary>
        /// A delegate for pre-processing exceptions before it is processed by the catcher.
        /// </summary>
        /// <param name="e">The UnhandledExceptionEventArgs passed to the catcher.</param>
        /// <returns>A value that indicates if other prehandlers and the catcher itself should be skipped.</returns>
        public delegate bool Prehandler(UnhandledExceptionEventArgs e);

        static bool suppressNext;
        static bool suppressed;
        static List<Prehandler> prehandlers = new List<Prehandler>();

        /// <summary>
        /// Gets a value that indicates if exception catching is suppressed.
        /// </summary>
        public static bool IsSuppressed
        {
            get
            {
                return suppressNext || suppressed;
            }
            internal set
            {
                if (!value) suppressNext = false;
                suppressed = value;
            }
        }

        /// <summary>
        /// Registers the catcher in the current AppDomain.
        /// </summary>
        public static void RegisterCatcher()
        {
            RegisterCatcher(AppDomain.CurrentDomain);
        }

        /// <summary>
        /// Registers the catcher in the provided AppDomain.
        /// </summary>
        /// <param name="domain">The AppDomain to register the catcher under.</param>
        public static void RegisterCatcher(AppDomain domain)
        {
            domain.UnhandledException += new UnhandledExceptionEventHandler(domain_UnhandledException);
        }

        /// <summary>
        /// Suppresses catching the next exception.
        /// </summary>
        public static void SuppressNext()
        {
            suppressNext = true;
        }

        /// <summary>
        /// Adds a prehandler.
        /// </summary>
        /// <param name="handler">The Prehandler to add.</param>
        public static void AddPreHandler(Prehandler handler)
        {
            prehandlers.Add(handler);
        }

        static void domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (suppressNext)
            {
                suppressNext = false;
                return;
            }
            if (suppressed) return;

            foreach (Prehandler prehandler in prehandlers)
            {
                try
                {
                    if (prehandler(e)) return;
                }
                catch { }
            }

            try
            {
                Exception exc = (Exception)e.ExceptionObject;
                if (MessageBox.Show("Dust: AET has encountered an unhandled exception. You can help the developer in solving this problem by giving them this exception message:" + Environment.NewLine + Environment.NewLine +
                                    exc.ToString() +
                                    Environment.NewLine + Environment.NewLine +
                                    "Extended information is available. Click \"Yes\" to paste the information into your clipboard, or \"No\" to dismiss this message.",
                                    "Dust: AET Unhandled Exception",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Exclamation)
                    == DialogResult.Yes)
                {
                    string extInfo = getExtendedInfo(exc);
                    RunAsSTAThread(() => Clipboard.SetText(extInfo, TextDataFormat.UnicodeText));
                    MessageBox.Show("The extended information has been placed on the clipboard. You may paste it in a message to the developer, say, on the game's Steam Community forum.", "Dust: AET Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                // welp
            }
        }

        // From http://stackoverflow.com/questions/5944605/c-sharp-clipboard-gettext
        /// <summary>
        /// Start an Action within an STA Thread
        /// </summary>
        /// <param name="goForIt"></param>
        internal static void RunAsSTAThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }

        static string getExtendedInfo(Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Environment Information");
            sb.AppendLine("=======================");
            sb.AppendLine("Report time: " + DateTime.Now);
            sb.AppendLine("OS: " + Environment.OSVersion);
            sb.AppendLine("Runtime: " + Environment.Version);

            ulong totalRam = 0;
            ManagementClass mgmt = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection moc = mgmt.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                totalRam += (ulong)mo["Capacity"];
            }
            sb.AppendLine("Total physical memory: " + totalRam + " bytes");

            mgmt = new ManagementClass("Win32_Processor");
            moc = mgmt.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                sb.AppendFormat("Processor: {0} ({1})" + Environment.NewLine, ((string)mo["Name"]).Trim(), mo["Description"]);
            }

            sb.AppendFormat("64-bit OS/process: {0}/{1}" + Environment.NewLine, Environment.Is64BitOperatingSystem ? "Yes" : "No", Environment.Is64BitProcess ? "Yes" : "No");

            sb.AppendLine("Graphics adapters:");
            mgmt = new ManagementClass("Win32_VideoController");
            moc = mgmt.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                sb.AppendFormat("\t{0} (Driver version: {1}, Video memory: {2} bytes)" + Environment.NewLine, mo["Description"], mo["DriverVersion"], mo["AdapterRAM"]);
            }

            sb.AppendLine();
            sb.AppendLine("Exception Information");
            sb.AppendLine("=====================");
            sb.AppendLine("Faulting assembly: " + exc.TargetSite.Module.Assembly.ToString());
            sb.AppendLine("Exception text: " + exc.ToString());

            return sb.ToString();
        }
    }
}
