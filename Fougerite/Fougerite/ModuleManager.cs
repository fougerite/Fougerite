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
        public ApiVersionAttribute(int major, int minor) : this(new Version(major, minor))
        {
        }
    }

    class ModuleManager
    {
        public static readonly Version ApiVersion = new Version(1, 0, 0, 0);
        private static string ModulesFolder = @".\Modules\";
        private static bool IsIgnoreVersion = true;
        private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
        private static readonly List<ModuleContainer> Modules = new List<ModuleContainer>();

        public static ReadOnlyCollection<ModuleContainer> Plugins
        {
            get { return new ReadOnlyCollection<ModuleContainer>(Modules); }
        }

        internal static void LoadModules()
        {
            Logger.Log("Loading modules...");
            string IgnoredPluginsFilePath = Path.Combine(ModulesFolder, "ignoredplugins.txt");

            List<string> IgnoredFiles = new List<string>();
            if (File.Exists(IgnoredPluginsFilePath))
                IgnoredFiles.AddRange(File.ReadAllLines(IgnoredPluginsFilePath));

            FileInfo[] FileInfos = new DirectoryInfo(ModulesFolder).GetFiles("*.dll");

            Dictionary<Module, Stopwatch> PluginInitWatches = new Dictionary<Module, Stopwatch>();
            foreach (FileInfo FileInfo in FileInfos)
            {
                Logger.Log("Module Found: " + FileInfo.Name);
                string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FileInfo.Name);
                if (IgnoredFiles.Contains(FileNameWithoutExtension))
                {
                    Logger.LogWarning(string.Format("{0} was ignored from being loaded.", FileNameWithoutExtension));
                    continue;
                }

                try
                {
                    Logger.Log("Loading " + FileInfo.Name);
                    Assembly Assembly;
                    // The plugin assembly might have been resolved by another plugin assembly already, so no use to
                    // load it again, but we do still have to verify it and create plugin instances.
                    if (!LoadedAssemblies.TryGetValue(FileNameWithoutExtension, out Assembly))
                    {
                        Assembly = Assembly.Load(File.ReadAllBytes(FileInfo.FullName));
                        LoadedAssemblies.Add(FileNameWithoutExtension, Assembly);
                    }

                    foreach (Type Type in Assembly.GetExportedTypes())
                    {
                        Logger.Log("Checking " + Type.FullName);
                        // Не сын, приватный или абстрактный
                        if (!Type.IsSubclassOf(typeof(Module)) || !Type.IsPublic || Type.IsAbstract)
                            continue;
                        Logger.Log("Checked " + Type.FullName);

                        object[] CustomAttributes = Type.GetCustomAttributes(typeof(ApiVersionAttribute), false);
                        if (CustomAttributes.Length == 0)
                            continue;
                        Logger.Log("Ok " + Type.FullName);

                        if (!IsIgnoreVersion)
                        {
                            var ApiVersionAttribute = (ApiVersionAttribute)CustomAttributes[0];
                            Version ApiVersion = ApiVersionAttribute.ApiVersion;
                            if (ApiVersion.Major != ApiVersion.Major || ApiVersion.Minor != ApiVersion.Minor)
                            {
                                Logger.LogWarning(string.Format("Plugin \"{0}\" is designed for a different API version ({1}) and was ignored.",
                                    Type.FullName, ApiVersion.ToString(2)));
                                continue;
                            }
                        }

                        Module PluginInstance = null;
                        try
                        {
                            Stopwatch InitTimeWatch = new Stopwatch();
                            InitTimeWatch.Start();
                            PluginInstance = (Module)Activator.CreateInstance(Type);
                            InitTimeWatch.Stop();
                            PluginInitWatches.Add(PluginInstance, InitTimeWatch);
                        }
                        catch (Exception ex)
                        {
                            // Broken plugins better stop the entire server init.
                            Logger.LogError(string.Format("Could not create an instance of plugin class \"{0}\". {1}", Type.FullName, ex));
                        }
                        if (PluginInstance != null)
                        {
                            Modules.Add(new ModuleContainer(PluginInstance));
                            Logger.Log("Loaded " + FileInfo.Name);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Broken assemblies / plugins better stop the entire server init.
                    Logger.LogError(string.Format("Failed to load assembly \"{0}\". {1}", FileInfo.Name, ex));
                }
            }

            IOrderedEnumerable<ModuleContainer> OrderedModuleSelector =
                from x in Plugins
                orderby x.Plugin.Order, x.Plugin.Name
                select x;

            foreach (ModuleContainer CurrentModule in OrderedModuleSelector)
            {
                Stopwatch InitTimeWatch = PluginInitWatches[CurrentModule.Plugin];
                InitTimeWatch.Start();

                try
                {
                    CurrentModule.Initialize();
                }
                catch (Exception ex)
                {
                    // Broken plugins better stop the entire server init.
                    Logger.LogError(string.Format(
                        "Plugin \"{0}\" has thrown an exception during initialization. {1}", CurrentModule.Plugin.Name, ex));
                }

                InitTimeWatch.Stop();
                Logger.Log(string.Format(
                    "Plugin {0} v{1} (by {2}) initiated.", CurrentModule.Plugin.Name, CurrentModule.Plugin.Version, CurrentModule.Plugin.Author));
            }
        }

        internal static void UnloadModules()
        {
            var ModuleUnloadWatches = new Dictionary<ModuleContainer, Stopwatch>();
            foreach (ModuleContainer ModuleContainer in Modules)
            {
                Stopwatch UnloadWatch = new Stopwatch();
                UnloadWatch.Start();

                try
                {
                    ModuleContainer.DeInitialize();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format(
                        "Plugin \"{0}\" has thrown an exception while being deinitialized:\n{1}", ModuleContainer.Plugin.Name, ex));
                }

                UnloadWatch.Stop();
                ModuleUnloadWatches.Add(ModuleContainer, UnloadWatch);
            }

            foreach (ModuleContainer ModuleContainer in Modules)
            {
                Stopwatch UnloadWatch = ModuleUnloadWatches[ModuleContainer];
                UnloadWatch.Start();

                try
                {
                    ModuleContainer.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format(
                        "Plugin \"{0}\" has thrown an exception while being disposed:\n{1}", ModuleContainer.Plugin.Name, ex));
                }

                UnloadWatch.Stop();
            }
        }
    }
}