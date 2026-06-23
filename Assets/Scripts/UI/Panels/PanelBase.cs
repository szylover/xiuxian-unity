// ============================================================
// PanelBase.cs — reusable runtime uGUI panel base
// ============================================================

using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.UI
{
    public abstract class PanelBase : IPanel
    {
        private Transform root;

        protected PanelBase(PanelId id, string title)
        {
            Id = id;
            Title = title;
        }

        public PanelId Id { get; }
        public string Title { get; }
        protected GameContext Context { get; private set; }
        protected Transform Root => root;

        public void Build(Transform parent, GameContext context)
        {
            Context = context;
            root = parent;
            Rebuild();
        }

        public virtual void OnGameEvent(in GameEvent gameEvent)
        {
            if (root != null && ShouldRefreshOn(gameEvent.Type)) Refresh();
        }

        protected virtual bool ShouldRefreshOn(GameEventType type) => type == GameEventType.PlayerChanged;

        protected virtual void Refresh() => Rebuild();

        protected void Rebuild()
        {
            if (root == null || Context == null) return;
            for (var i = root.childCount - 1; i >= 0; i--)
                Object.Destroy(root.GetChild(i).gameObject);
            BuildContent(root);
        }

        protected abstract void BuildContent(Transform parent);
    }
}
