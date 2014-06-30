using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fougerite
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ApiVersionAttribute : Attribute
    {
        public Version ApiVersion;
        public ApiVersionAttribute(Version version)
        {
            this.ApiVersion = version;
        }
        public ApiVersionAttribute(int major, int minor)
            : this(new Version(major, minor))
        {
        }
    }

    class ModuleManager
    {
        public static readonly Version ApiVersion = new Version(1, 0, 0, 0);
        private static string ModulesFolder = @".\Modules\";
        private static bool IgnoreVersion = false;
        private static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        private static readonly List<ModuleContainer> plugins = new List<ModuleContainer>();

        public static ReadOnlyCollection<ModuleContainer> Plugins
        {
            get { return new ReadOnlyCollection<ModuleContainer>(plugins); }
        }

        internal static void LoadModules()
        {

            string IgnoredPluginsFilePath = Path.Combine(ModulesFolder, "ignoredplugins.txt");

            List<string> IgnoredFiles = new List<string>();
            if (File.Exists(IgnoredPluginsFilePath))
                IgnoredFiles.AddRange(File.ReadAllLines(IgnoredPluginsFilePath));

            FileInfo[] FileInfos = new DirectoryInfo(ModulesFolder).GetFiles("*.dll");

            Dictionary<Module, Stopwatch> pluginInitWatches = new Dictionary<Module, Stopwatch>();
            foreach (FileInfo fileInfo in FileInfos)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                if (IgnoredFiles.Contains(fileNameWithoutExtension))
                {
                    Logger.LogWarning(string.Format("{0} was ignored from being loaded.", fileNameWithoutExtension));
                    continue;
                }

                try
                {
                    Assembly assembly;
                    // The plugin assembly might have been resolved by another plugin assembly already, so no use to
                    // load it again, but we do still have to verify it and create plugin instances.
                    if (!loadedAssemblies.TryGetValue(fileNameWithoutExtension, out assembly))
                    {
                        assembly = Assembly.Load(File.ReadAllBytes(fileInfo.FullName));
                        loadedAssemblies.Add(fileNameWithoutExtension, assembly);
                    }

                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (!type.IsSubclassOf(typeof(Module)) || !type.IsPublic || type.IsAbstract)
                            continue;
                        object[] customAttributes = type.GetCustomAttributes(typeof(ApiVersionAttribute), false);
                        if (customAttributes.Length == 0)
                            continue;

                        if (!IgnoreVersion)
                        {
                            var apiVersionAttribute = (ApiVersionAttribute)customAttributes[0];
                            Version apiVersion = apiVersionAttribute.ApiVersion;
                            if (apiVersion.Major != ApiVersion.Major || apiVersion.Minor != ApiVersion.Minor)
                            {
                                Logger.LogWarning(string.Format("Plugin \"{0}\" is designed for a different API version ({1}) and was ignored.",
                                    type.FullName, apiVersion.ToString(2)));
                                continue;
                            }
                        }

                        Module PluginInstance;
                        try
                        {
                            Stopwatch initTimeWatch = new Stopwatch();
                            initTimeWatch.Start();
                            PluginInstance = (Module)Activator.CreateInstance(type);
                            initTimeWatch.Stop();
                            pluginInitWatches.Add(PluginInstance, initTimeWatch);
                        }
                        catch (Exception ex)
                        {
                            // Broken plugins better stop the entire server init.
                            throw new InvalidOperationException(
                                string.Format("Could not create an instance of plugin class \"{0}\".", type.FullName), ex);
                        }
                        plugins.Add(new ModuleContainer(PluginInstance));
                    }
                }
                catch (Exception ex)
                {
                    // Broken assemblies / plugins better stop the entire server init.
                    throw new InvalidOperationException(
                        string.Format("Failed to load assembly \"{0}\".", fileInfo.Name), ex);
                }
            }

            IOrderedEnumerable<ModuleContainer> orderedPluginSelector =
                from x in Plugins
                orderby x.Plugin.Order, x.Plugin.Name
                select x;

            foreach (ModuleContainer current in orderedPluginSelector)
            {
                Stopwatch initTimeWatch = pluginInitWatches[current.Plugin];
                initTimeWatch.Start();

                try
                {
                    current.Initialize();
                }
                catch (Exception ex)
                {
                    // Broken plugins better stop the entire server init.
                    throw new InvalidOperationException(string.Format(
                        "Plugin \"{0}\" has thrown an exception during initialization.", current.Plugin.Name), ex);
                }

                initTimeWatch.Stop();
                Logger.Log(string.Format(
                    "Plugin {0} v{1} (by {2}) initiated.", current.Plugin.Name, current.Plugin.Version, current.Plugin.Author));
            }
        }

        internal static void UnloadPlugins()
        {
            var pluginUnloadWatches = new Dictionary<ModuleContainer, Stopwatch>();
            foreach (ModuleContainer pluginContainer in plugins)
            {
                Stopwatch unloadWatch = new Stopwatch();
                unloadWatch.Start();

                try
                {
                    pluginContainer.DeInitialize();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format(
                        "Plugin \"{0}\" has thrown an exception while being deinitialized:\n{1}", pluginContainer.Plugin.Name, ex));
                }

                unloadWatch.Stop();
                pluginUnloadWatches.Add(pluginContainer, unloadWatch);
            }

            foreach (ModuleContainer pluginContainer in plugins)
            {
                Stopwatch unloadWatch = pluginUnloadWatches[pluginContainer];
                unloadWatch.Start();

                try
                {
                    pluginContainer.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format(
                        "Plugin \"{0}\" has thrown an exception while being disposed:\n{1}", pluginContainer.Plugin.Name, ex));
                }

                unloadWatch.Stop();
            }
        }
    }
}