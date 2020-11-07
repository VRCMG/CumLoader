using System;
#pragma warning disable 0108

namespace CumLoader
{
    public abstract class CumMod : CumBase
    {
        [Obsolete()]
        public CumModInfoAttribute InfoAttribute { get => LegacyModInfo; }
        [Obsolete()]
        public CumModGameAttribute[] GameAttributes { get => LegacyModGames; }

        public virtual void OnLevelIsLoading() {}
        public virtual void OnLevelWasLoaded(int level) {}
        public virtual void OnLevelWasInitialized(int level) {}
        public virtual void OnFixedUpdate() {}
    }
}