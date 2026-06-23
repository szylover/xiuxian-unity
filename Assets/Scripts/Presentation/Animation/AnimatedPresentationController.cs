// ============================================================
// AnimatedPresentationController.cs — exposes #13 transition request hooks
// ============================================================

using System;
using Xiuxian.App;

namespace Xiuxian.Presentation.Animation
{
    public sealed class AnimatedPresentationController : PresentationController
    {
        public AnimatedPresentationController(GameContext context, PortraitSystem portraitSystem = null, SceneSystem sceneSystem = null)
            : base(context, portraitSystem, sceneSystem)
        {
        }

        public event EventHandler<PortraitTransitionRequestEventArgs> PortraitTransitionRequested;
        public event EventHandler<SceneTransitionRequestEventArgs> SceneTransitionRequested;

        protected override void RequestPortraitTransition(PortraitDescriptor previous, PortraitDescriptor next)
            => PortraitTransitionRequested?.Invoke(this, new PortraitTransitionRequestEventArgs(previous, next));

        protected override void RequestSceneTransition(SceneDescriptor previous, SceneDescriptor next)
            => SceneTransitionRequested?.Invoke(this, new SceneTransitionRequestEventArgs(previous, next));
    }

    public sealed class PortraitTransitionRequestEventArgs : EventArgs
    {
        public PortraitTransitionRequestEventArgs(PortraitDescriptor previous, PortraitDescriptor next)
        {
            Previous = previous;
            Next = next;
        }

        public PortraitDescriptor Previous { get; }
        public PortraitDescriptor Next { get; }
    }

    public sealed class SceneTransitionRequestEventArgs : EventArgs
    {
        public SceneTransitionRequestEventArgs(SceneDescriptor previous, SceneDescriptor next)
        {
            Previous = previous;
            Next = next;
        }

        public SceneDescriptor Previous { get; }
        public SceneDescriptor Next { get; }
    }
}
