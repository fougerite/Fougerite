using System;
using System.Collections.Generic;
using System.Text;

namespace Fougerite
{
    public abstract class Module : IDisposable
    {

        public virtual string Name
        {
            get
            {
                return "None";
            }
        }

        public virtual Version Version
        {
            get
            {
                return new Version(1, 0);
            }
        }

        public virtual string Author
        {
            get
            {
                return "None";
            }
        }

        public virtual string Description
        {
            get
            {
                return "None";
            }
        }

        public virtual bool Enabled
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }
        public virtual string UpdateURL
        {
            get
            {
                return "";
            }
        }

        ~Module()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void Initialize();
    }
}