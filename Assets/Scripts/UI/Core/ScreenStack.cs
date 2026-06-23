// ============================================================
// ScreenStack.cs — simple full-screen navigator
// ============================================================

using System;
using UnityEngine;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public sealed class ScreenStack
    {
        private readonly GameContext context;
        private readonly Canvas canvas;
        private UiScreen current;

        public ScreenStack(GameContext context)
        {
            this.context = context;
            canvas = UIBuilder.CreateCanvas();
        }

        public T Show<T>(Action<T> configure = null) where T : UiScreen
        {
            if (current != null) UnityEngine.Object.Destroy(current.gameObject);
            var go = UIBuilder.Rect(typeof(T).Name, canvas.transform);
            UIBuilder.Stretch(go.GetComponent<RectTransform>());
            var screen = go.AddComponent<T>();
            current = screen;
            screen.SetDependencies(context, this);
            configure?.Invoke(screen);
            screen.BuildScreen();
            return screen;
        }
    }
}
