// ============================================================
// UiScreen.cs — base runtime screen contract
// ============================================================

using UnityEngine;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public abstract class UiScreen : MonoBehaviour
    {
        protected GameContext Context { get; private set; }
        protected ScreenStack Navigator { get; private set; }

        public void SetDependencies(GameContext context, ScreenStack navigator)
        {
            Context = context;
            Navigator = navigator;
        }

        public void BuildScreen() => Build();

        protected abstract void Build();
    }
}
