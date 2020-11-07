using System;
#pragma warning disable 0108

namespace CumLoader
{
    public abstract class CumPlugin : CumBase
    {
        [Obsolete()]
        public CumPluginInfoAttribute InfoAttribute { get => LegacyPluginInfo; }
        [Obsolete()]
        public CumPluginGameAttribute[] GameAttributes { get => LegacyPluginGames; }

        public virtual void OnPreInitialization() { }
    }
}