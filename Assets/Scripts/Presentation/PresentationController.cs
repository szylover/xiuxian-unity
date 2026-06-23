// ============================================================
// PresentationController.cs — binds game state/events to presentation descriptors
// ============================================================

using System;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.Presentation
{
    public class PresentationController : IDisposable
    {
        private readonly GameContext context;
        private readonly PortraitSystem portraitSystem;
        private readonly SceneSystem sceneSystem;

        public PresentationController(GameContext context, PortraitSystem portraitSystem = null, SceneSystem sceneSystem = null)
        {
            this.context = context;
            this.portraitSystem = portraitSystem ?? new PortraitSystem();
            this.sceneSystem = sceneSystem ?? new SceneSystem();
            if (context?.Bus != null) context.Bus.Subscribe(OnGameEvent);
        }

        public bool Enabled { get; set; } = true;
        public PortraitDescriptor CurrentPortrait { get; private set; }
        public SceneDescriptor CurrentScene { get; private set; }

        public event EventHandler<PortraitChangedEventArgs> PortraitChanged;
        public event EventHandler<SceneChangedEventArgs> SceneChanged;

        public void RefreshAll()
        {
            if (!Enabled) return;
            RefreshPortrait();
            RefreshScene();
        }

        public virtual void RefreshPortrait()
        {
            if (!Enabled || context?.CurrentPlayer == null) return;
            var next = portraitSystem.Resolve(context.CurrentPlayer);
            RequestPortraitTransition(CurrentPortrait, next);
            CurrentPortrait = next;
            OnPortraitChanged(next);
        }

        public virtual void RefreshScene()
        {
            if (!Enabled || context?.CurrentPlayer == null) return;
            var next = sceneSystem.Resolve(context);
            RequestSceneTransition(CurrentScene, next);
            CurrentScene = next;
            OnSceneChanged(next);
        }

        /// <summary>Issue #13 transition systems can override or subscribe here before the view swaps sprites.</summary>
        protected virtual void RequestPortraitTransition(PortraitDescriptor previous, PortraitDescriptor next) { }

        /// <summary>Issue #13/#14 can override this to start fades, camera moves, ambiance, or VFX on scene changes.</summary>
        protected virtual void RequestSceneTransition(SceneDescriptor previous, SceneDescriptor next) { }

        protected virtual void OnPortraitChanged(PortraitDescriptor portrait)
            => PortraitChanged?.Invoke(this, new PortraitChangedEventArgs(portrait));

        protected virtual void OnSceneChanged(SceneDescriptor scene)
            => SceneChanged?.Invoke(this, new SceneChangedEventArgs(scene));

        private void OnGameEvent(in GameEvent gameEvent)
        {
            if (!Enabled) return;
            if (IsPortraitEvent(gameEvent.Type)) RefreshPortrait();
            if (IsSceneEvent(gameEvent.Type)) RefreshScene();
        }

        private static bool IsPortraitEvent(GameEventType type)
            => type == GameEventType.PlayerCreated
            || type == GameEventType.PlayerLoaded
            || type == GameEventType.PlayerChanged
            || type == GameEventType.RealmChanged;

        private static bool IsSceneEvent(GameEventType type)
            => type == GameEventType.PlayerCreated
            || type == GameEventType.PlayerLoaded
            || type == GameEventType.PlayerChanged
            || type == GameEventType.DatabaseLoaded
            || type == GameEventType.MapChanged
            || type == GameEventType.RegionChanged
            || type == GameEventType.NpcChanged
            || type == GameEventType.RealmChanged;

        public virtual void Dispose()
        {
            if (context?.Bus != null) context.Bus.Unsubscribe(OnGameEvent);
        }
    }
}
