using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using DustAetPatchingPlatform;
using NativeAssemblerInjection;

namespace SwitchLang
{
    class SwitchLang : ILoader
    {
        static readonly string LangOptFileName = "lang.txt";

        public string Name
        {
            get { return "Language Switcher"; }
        }

        public int Priority
        {
            get { return 1000; }
        }

        public void Load()
        {
            if (!File.Exists(LangOptFileName)) return;
            string lang = File.ReadAllText(LangOptFileName).Trim();
            if (string.IsNullOrWhiteSpace(lang) || lang.ToLower().Contains("system")) return;

            Type myType = GetType();

            try
            {
                CultureInfo ci = CultureInfo.GetCultureInfo(lang);
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = ci;
                MethodUtil.ReplaceMethod(myType.GetMethod("SetResourcesReplacement"), typeof(Dust.Game1).GetMethod("SetResources"));

                foreach (Type t in typeof(Dust.Game1).Assembly.GetTypes().Where((tt) => tt.FullName.StartsWith("Dust.Strings")))
                {
                    PropertyInfo cultureProp = t.GetProperty("Culture", BindingFlags.Static | BindingFlags.NonPublic);
                    cultureProp.SetValue(null, ci, null);
                }
            }
            catch (CultureNotFoundException)
            {
                MessageBox.Show("Could not find specified culture. Please check your " + LangOptFileName + " file.", "Dust: AET Language Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SetResourcesReplacement()
        {
            // Do nothing. We've already set the culture.
        }
    }
}
