using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DustAetPatchingPlatform;

namespace Troubleshooter
{
    class Troubleshooter : ILoader
    {
        static string msgBoxTitle = "Dust: AET Crash Troubleshooter";

        public string Name
        {
            get { return "Troubleshooter"; }
        }

        public int Priority
        {
            get { return 0; }
        }

        public void Load()
        {
            ExceptionCatcher.AddPreHandler(troubleshootProc);
        }

        bool troubleshootProc(UnhandledExceptionEventArgs e)
        {
            // As the number of conditions that can be troubleshooted increases, I'll migrate each troubleshooter to a separate class.
            Exception ex = e.ExceptionObject as Exception;
            if (ex is InvalidOperationException)
            {
                // Probably corrupted files
                if (ex.Message.Contains("Song playback failed") || ex.Message.Contains("Error decompressing content data"))
                {
                    displayBadFilesMessage();
                    return true;
                }
            }
            else if (ex is System.IO.IOException)
            {
                if (ex.StackTrace.Contains("StorageContainer.FinishCreation"))
                {
                    MessageBox.Show("Could not read or create save file/folder. Please check that you have read and write access to the \"savedgames\" folder in your My Documents folder.",
                        msgBoxTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return true;
                }
                else
                {
                    displayBadFilesMessage();
                    return true;
                }
            }
            else if (ex is NullReferenceException)
            {
                if (ex.StackTrace.Contains("InputKey.IsPressed"))
                {
                    MessageBox.Show("You have encountered a known bug, with a known fix. Please make sure the PrevStateFix patch is in your patches folder.", msgBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return true;
                }
            }
            return false;
        }

        void displayBadFilesMessage()
        {
            string mboxMessage = "Your game files are corrupted. ";
            if (Dust.Game1.isSteam)
            {
                mboxMessage += "Please repair the game files by opening your Steam Library, right clicking on \"Dust: An Elysian Tail\", and click Properties->Local Files->Verify Integrity of Game Cache..." +
                    Environment.NewLine + Environment.NewLine + "Be sure to reinstall the Patching Platform after verifying cache, because Steam will overwrite it.";
            }
            else
            {
                mboxMessage += "Please reinstall the game.";
            }
            MessageBox.Show(mboxMessage, msgBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
