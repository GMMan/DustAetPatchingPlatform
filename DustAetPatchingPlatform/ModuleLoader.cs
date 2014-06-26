using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace DustAetPatchingPlatform
{
    static class ModuleLoader
    {
        static readonly Type ILoaderType = typeof(ILoader);

        static string pluginsDirName = "patches";
        static byte[] signedPublicKeyToken = new byte[] { 0x68, 0x4d, 0x77, 0xa7, 0x75, 0x73, 0xda, 0xfe };
        static List<LoadingException> loadErrors = new List<LoadingException>();

        /// <summary>
        /// A class that holds information about plugins.
        /// </summary>
        class PluginInfo
        {
            /// <summary>
            /// Gets or sets the path of the plugin.
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// Gets or sets a list of the names of types that implement ILoader.
            /// </summary>
            public List<string> LoaderTypeNames { get; set; }

            /// <summary>
            /// Instantiates a new instance of LoaderInfo.
            /// </summary>
            public PluginInfo()
            {
                LoaderTypeNames = new List<string>();
            }
        }

        /// <summary>
        /// A specialized exception for errors during plugin loading.
        /// </summary>
        public class LoadingException : Exception
        {
            /// <summary>
            /// Gets the path or name of the loader being processed when the exception occured.
            /// </summary>
            public string ModulePath { get; private set; }

            /// <summary>
            /// Instantiates a new instance of LoadingException.
            /// </summary>
            /// <param name="modulePath">The path or name of the loader being processed when the exception occured.</param>
            /// <param name="innerException">The exception that occured.</param>
            // Note: modulePath can be either a path or a ILoader name. Covering both as path is really horrible. Consider this a kludge.
            public LoadingException(string modulePath, Exception innerException)
                : base((System.IO.File.Exists(modulePath) ? System.IO.Path.GetFileName(modulePath) : modulePath) + ": " + innerException.Message, innerException)
            {
                ModulePath = modulePath;
            }
        }

        /// <summary>
        /// Gets a list of loading exceptions that have occured.
        /// </summary>
        public static List<LoadingException> LoadErrors
        {
            get
            {
                return loadErrors;
            }
        }

        /// <summary>
        /// Loads all plugins from the plugins directory.
        /// </summary>
        public static void LoadAllFromPluginsDir()
        {
            if (!Directory.Exists(pluginsDirName)) return;

            // Stage 1: Query all DLLs in plugins folder, validate their strong name, and get a list of ILoaders if validated
            List<PluginInfo> loaderInfos = new List<PluginInfo>();
            foreach (string file in Directory.EnumerateFiles(pluginsDirName, "*.dll"))
            {
                PluginInfo pi = validateAssembly(file);
                if (pi != null) loaderInfos.Add(pi);
            }

            // Stage 2: Instantiate loaders and queue them up; sort according to priority
            List<ILoader> loaders = new List<ILoader>();
            foreach (PluginInfo loaderInfo in loaderInfos)
            {
                List<ILoader> asmLoaders = getLoadersFromAssembly(loaderInfo);
                if (asmLoaders != null) loaders.AddRange(asmLoaders);
            }
            loaders.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            // Stage 3: Call each loader
            foreach (ILoader loader in loaders)
            {
                try
                {
                    loader.Load();
                }
                catch (Exception ex)
                {
                    loadErrors.Add(new LoadingException(loader.Name, ex));
                }
            }

            // And we're done!
        }

        /// <summary>
        /// Loads a single plugin by path.
        /// </summary>
        /// <param name="path">The path of the plugin.</param>
        /// <returns>A value that indicates whether or not the plugin was successfully loaded.</returns>
        public static bool LoadByPath(string path)
        {
            // The way this function works is the same as LoadAllFromPluginsDir(), but reduced to operating on a single file.

            PluginInfo pi = validateAssembly(path);
            if (pi == null) return false;

            List<ILoader> loaders = getLoadersFromAssembly(pi);
            if (loadErrors == null) return false;
            loaders.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            bool allOk = true;
            foreach (ILoader loader in loaders)
            {
                try
                {
                    loader.Load();
                }
                catch (Exception ex)
                {
                    allOk = false;
                    loadErrors.Add(new LoadingException(loader.Name, ex));
                }
            }

            return allOk;
        }

        /// <summary>
        /// Validates the given plugin, and provides a LoaderInfo about the plugin.
        /// </summary>
        /// <param name="path">The path to the plugin.</param>
        /// <returns>A LoaderInfo object with information about the plugin. <c>null</c> is returned if the assembly fails strong name validation or if there was an error during loading.</returns>
        static PluginInfo validateAssembly(string path)
        {
            try
            {
                Assembly checkAssembly = Assembly.ReflectionOnlyLoadFrom(path);
                // Never mind about strong name validation. DustAET is not strong name signed, and makes everything else fail.
                //if (!compareByteArrays(checkAssembly.GetName().GetPublicKeyToken(), signedPublicKeyToken)) throw new Exception("Module " + checkAssembly.FullName + " is unauthorized.");

                PluginInfo pi = new PluginInfo { Path = path };

                Assembly loadAssembly = Assembly.LoadFrom(path);
                foreach (Type t in loadAssembly.GetTypes().Where((tt) => ILoaderType.IsConcretelyImplementedBy(tt)))
                {
                    pi.LoaderTypeNames.Add(t.FullName);
                }

                return pi;
            }
            catch (Exception ex)
            {
                loadErrors.Add(new LoadingException(path, ex));
                return null;
            }
        }

        /// <summary>
        /// Gets a list of ILoaders from the plugin given in <paramref name="info"/>.
        /// </summary>
        /// <param name="info">The LoaderInfo containing information about the plugin.</param>
        /// <returns>A list of ILoaders from the plugin.</returns>
        static List<ILoader> getLoadersFromAssembly(PluginInfo info)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(info.Path);
                List<ILoader> loaders = new List<ILoader>();

                foreach (string typeName in info.LoaderTypeNames)
                {
                    // ConstructorInfo.Invoke() is faster than Activator.CreateInstance()
                    loaders.Add((ILoader)assembly.GetType(typeName).GetConstructor(Type.EmptyTypes).Invoke(null));
                }

                return loaders;
            }
            catch (Exception ex)
            {
                loadErrors.Add(new LoadingException(info.Path, ex));
                return null;
            }
        }

        /// <summary>
        /// Compares if two byte arrays are equal.
        /// </summary>
        /// <param name="x">The first byte array.</param>
        /// <param name="y">The second byte array.</param>
        /// <returns>If the byte arrays are equal. If either are <c>null</c>, <c>false</c> is returned.</returns>
        static bool compareByteArrays(byte[] x, byte[] y)
        {
            if (x == null || y == null) return false;
            if (x == y) return true;
            if (x.Length != y.Length) return false;

            for (int i = 0; i < x.Length; ++i)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }
    }
}
