using System;
using System.Collections.Generic;
using System.Text;

namespace Fougerite
{
    public class ModuleContainer : IDisposable
    {
        public Module Plugin
        {
            get;
            protected set;
        }

        public bool Initialized
        {
            get;
            protected set;
        }

        public bool Dll
        {
            get;
            set;
        }

        public ModuleContainer(Module plugin) : this(plugin, true)
        {
        }

        public ModuleContainer(Module plugin, bool dll)
        {
            this.Plugin = plugin;
            this.Initialized = false;
            this.Dll = dll;
        }

        public void Initialize()
        {
            this.Plugin.Initialize();
            this.Initialized = true;
        }

        public void DeInitialize()
        {
            this.Initialized = false;
        }

        public void Dispose()
        {
            this.Plugin.Dispose();
        }
    }
}
