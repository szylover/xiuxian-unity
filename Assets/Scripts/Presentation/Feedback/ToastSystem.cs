// ============================================================
// ToastSystem.cs — event-driven toast entry point
// ============================================================

using System;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.Presentation.Feedback
{
    public sealed class ToastSystem : IDisposable
    {
        private readonly GameContext context;
        private readonly ToastContainer container;

        public ToastSystem(GameContext context, FeedbackOverlay overlay)
        {
            this.context = context;
            container = new ToastContainer(overlay);
            if (context?.Bus != null) context.Bus.Subscribe(GameEventType.ToastRequested, OnToastRequested);
        }

        public void Show(string text, ToastSeverity severity = ToastSeverity.Info, float duration = 0f)
            => container?.Enqueue(text, severity, duration);

        public void Dispose()
        {
            if (context?.Bus != null) context.Bus.Unsubscribe(GameEventType.ToastRequested, OnToastRequested);
            container?.Dispose();
        }

        private void OnToastRequested(in GameEvent gameEvent)
        {
            if (gameEvent.Payload is ToastRequest request) Show(request.Text, request.Severity, request.DurationSeconds);
            else if (gameEvent.Payload is string text) Show(text, ToastSeverity.Info);
        }
    }
}
