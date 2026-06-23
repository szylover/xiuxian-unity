// ============================================================
// AnimationDirector.cs — binds game events and presentation hooks to motion
// ============================================================

using System;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;

namespace Xiuxian.Presentation.Animation
{
    public sealed class AnimationTargets
    {
        public Transform Root;
        public RectTransform StatusBar;
        public RectTransform CombatFeedbackArea;
        public PortraitView PortraitView;
        public SceneView SceneView;
    }

    public sealed class AnimationDirector : IDisposable
    {
        private readonly GameContext context;
        private readonly PresentationController presentationController;
        private readonly AnimatedPresentationController animatedController;
        private readonly AnimationTargets targets;

        public AnimationDirector(GameContext context, PresentationController presentationController, AnimationTargets targets)
        {
            this.context = context;
            this.presentationController = presentationController;
            animatedController = presentationController as AnimatedPresentationController;
            this.targets = targets ?? new AnimationTargets();

            if (context?.Bus != null) context.Bus.Subscribe(OnGameEvent);
            if (presentationController != null)
            {
                presentationController.PortraitChanged += OnPortraitChanged;
                presentationController.SceneChanged += OnSceneChanged;
            }
            if (animatedController != null) animatedController.SceneTransitionRequested += OnSceneTransitionRequested;
        }

        /// <summary>Issue #14 can attach particle VFX to these beat hooks without changing game logic.</summary>
        public event Action<ThemePalette> BreakthroughSucceeded;
        public event Action<ThemePalette> BreakthroughFailed;
        public event Action<ThemePalette> RealmAdvanced;
        /// <summary>Issue #16 can attach floating text or real screen/camera shake to this lightweight UI feedback hook.</summary>
        public event Action<ThemePalette> CombatFeedbackRequested;
        public event Action GameOverFadeRequested;

        public void Dispose()
        {
            if (context?.Bus != null) context.Bus.Unsubscribe(OnGameEvent);
            if (presentationController != null)
            {
                presentationController.PortraitChanged -= OnPortraitChanged;
                presentationController.SceneChanged -= OnSceneChanged;
            }
            if (animatedController != null) animatedController.SceneTransitionRequested -= OnSceneTransitionRequested;
        }

        private void OnSceneTransitionRequested(object sender, SceneTransitionRequestEventArgs e)
        {
            if (e?.Next == null) return;
            UiTransitionLibrary.PlaySceneTransition(targets.SceneView, e.Next.Palette);
        }

        private void OnPortraitChanged(object sender, PortraitChangedEventArgs e)
        {
            if (e?.Portrait == null) return;
            UiTransitionLibrary.PlayPortraitSwap(targets.PortraitView, e.Portrait.Palette);
        }

        private void OnSceneChanged(object sender, SceneChangedEventArgs e)
        {
            if (animatedController != null) return;
            if (e?.Scene == null) return;
            UiTransitionLibrary.PlaySceneTransition(targets.SceneView, e.Scene.Palette);
        }

        private void OnGameEvent(in GameEvent gameEvent)
        {
            var palette = CurrentPalette();
            switch (gameEvent.Type)
            {
                case GameEventType.BreakthroughChanged:
                    PlayBreakthroughFailure(palette);
                    break;
                case GameEventType.RealmChanged:
                    PlayBreakthroughSuccess(palette);
                    PlayRealmAdvance(palette);
                    break;
                case GameEventType.CultivationChanged:
                    UiTransitionLibrary.PlayCultivationPulse(targets.StatusBar, palette.Accent);
                    break;
                case GameEventType.CombatChanged:
                    UiTransitionLibrary.PlayCombatHit(targets.CombatFeedbackArea ?? targets.StatusBar, Color.red);
                    CombatFeedbackRequested?.Invoke(palette);
                    break;
                case GameEventType.GameOver:
                    UiTransitionLibrary.PlayGameOverFade(targets.Root);
                    GameOverFadeRequested?.Invoke();
                    break;
            }
        }

        private void PlayBreakthroughSuccess(ThemePalette palette)
        {
            var target = targets.StatusBar;
            UiTransitionLibrary.PlayBuildUp(target, palette.Primary);
            UiTransitionLibrary.PlaySuccessBurst(target, palette.Accent);
            BreakthroughSucceeded?.Invoke(palette);
        }

        private void PlayBreakthroughFailure(ThemePalette palette)
        {
            var target = targets.StatusBar;
            UiTransitionLibrary.PlayFailureShudder(target);
            UiTransitionLibrary.PlayCombatHit(target, palette.Secondary);
            BreakthroughFailed?.Invoke(palette);
        }

        private void PlayRealmAdvance(ThemePalette palette)
        {
            UiTransitionLibrary.PlayRealmFlourish(targets.Root, palette.Accent);
            if (targets.PortraitView?.Root != null)
                UiTransitionLibrary.PlaySuccessBurst(targets.PortraitView.Root.GetComponent<RectTransform>(), palette.Accent);
            RealmAdvanced?.Invoke(palette);
        }

        private ThemePalette CurrentPalette()
        {
            var realm = context?.CurrentPlayer?.RealmIndex ?? 0;
            return RealmTheme.ForRealm(realm);
        }
    }
}
