using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Fougerite
{
    public class ModuleContainer : IDisposable
    {
        public readonly Module Plugin;

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
            Contract.Requires(plugin != null);
        }

        public ModuleContainer(Module plugin, bool dll)
        {
            Contract.Requires(plugin != null);
            this.Plugin = plugin;
            this.Initialized = false;
            this.Dll = dll;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(Plugin != null);
        }

        public void Initialize()
        {
            this.Plugin.Initialize();
            this.Initialized = true;
        }

        public void DeInitialize()
        {
            this.Initialized = false;
            this.Plugin.DeInitialize();
        }

        public void Dispose()
        {
            this.Plugin.Dispose();
        }
    }
}
