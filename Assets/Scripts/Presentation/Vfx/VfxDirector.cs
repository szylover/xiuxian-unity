// ============================================================
// VfxDirector.cs — binds animation hooks and game events to VFX
// ============================================================

using System;
using UnityEngine;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Presentation.Animation;

namespace Xiuxian.Presentation.Vfx
{
    public sealed class VfxDirector : IDisposable
    {
        private readonly GameContext context;
        private readonly AnimationDirector animationDirector;
        private readonly AnimationTargets targets;
        private readonly VfxOverlay overlay;
        private VfxInstance ambient;

        public VfxDirector(GameContext context, AnimationDirector animationDirector, VfxOverlay overlay, AnimationTargets targets)
        {
            this.context = context;
            this.animationDirector = animationDirector;
            this.overlay = overlay;
            this.targets = targets ?? new AnimationTargets();
            Enabled = VfxSettings.Enabled;

            if (animationDirector != null)
            {
                animationDirector.BreakthroughSucceeded += OnBreakthroughSucceeded;
                animationDirector.BreakthroughFailed += OnBreakthroughFailed;
                animationDirector.RealmAdvanced += OnRealmAdvanced;
                animationDirector.CombatFeedbackRequested += OnCombatFeedbackRequested;
            }
            if (context?.Bus != null) context.Bus.Subscribe(OnGameEvent);
            RefreshAmbient(CurrentPalette());
        }

        public bool Enabled
        {
            get => overlay != null && overlay.Enabled;
            set
            {
                if (overlay == null) return;
                VfxSettings.Enabled = value;
                overlay.SetEnabled(value);
                if (value) RefreshAmbient(CurrentPalette());
            }
        }

        public void Dispose()
        {
            if (animationDirector != null)
            {
                animationDirector.BreakthroughSucceeded -= OnBreakthroughSucceeded;
                animationDirector.BreakthroughFailed -= OnBreakthroughFailed;
                animationDirector.RealmAdvanced -= OnRealmAdvanced;
                animationDirector.CombatFeedbackRequested -= OnCombatFeedbackRequested;
            }
            if (context?.Bus != null) context.Bus.Unsubscribe(OnGameEvent);
            overlay?.StopAll();
        }

        private void OnBreakthroughSucceeded(ThemePalette palette)
        {
            if (!CanPlay()) return;
            Play(targets.PortraitView?.Root?.GetComponent<RectTransform>() ?? targets.StatusBar, VfxLibrary.BreakthroughSuccess(palette));
            Play(targets.StatusBar, VfxLibrary.BreakthroughSuccess(palette));
        }

        private void OnBreakthroughFailed(ThemePalette palette)
        {
            if (!CanPlay()) return;
            Play(targets.StatusBar, VfxLibrary.BreakthroughFailure(palette));
        }

        private void OnRealmAdvanced(ThemePalette palette)
        {
            if (!CanPlay()) return;
            Play(targets.Root as RectTransform ?? targets.StatusBar, VfxLibrary.RealmAdvance(palette));
            RefreshAmbient(palette);
        }

        private void OnCombatFeedbackRequested(CombatFeedbackRequest request)
        {
            if (!CanPlay()) return;
            Play(targets.CombatFeedbackArea ?? targets.StatusBar, VfxLibrary.CombatHit(request?.Palette ?? CurrentPalette()));
        }

        private void OnGameEvent(in GameEvent gameEvent)
        {
            if (!CanPlay()) return;
            var palette = CurrentPalette();
            switch (gameEvent.Type)
            {
                case GameEventType.AlchemyChanged:
                    Play(targets.CombatFeedbackArea ?? targets.StatusBar, VfxLibrary.Alchemy(palette));
                    break;
                case GameEventType.SmithingChanged:
                    Play(targets.CombatFeedbackArea ?? targets.StatusBar, VfxLibrary.Smithing(palette));
                    break;
                case GameEventType.AscensionChanged:
                    Play(targets.SceneView?.Root?.GetComponent<RectTransform>() ?? targets.Root as RectTransform, VfxLibrary.Ascension());
                    break;
                case GameEventType.MapChanged:
                case GameEventType.RegionChanged:
                case GameEventType.RealmChanged:
                    RefreshAmbient(palette);
                    break;
            }
        }

        private void RefreshAmbient(ThemePalette palette)
        {
            if (!CanPlay()) return;
            ambient?.Release();
            var anchor = targets.SceneView?.Root?.GetComponent<RectTransform>() ?? targets.Root as RectTransform;
            ambient = overlay.Play(anchor, VfxLibrary.Ambient(palette));
        }

        private void Play(RectTransform anchor, VfxEffectDescriptor descriptor)
        {
            if (anchor == null || descriptor == null) return;
            overlay.Play(anchor, descriptor);
        }

        private bool CanPlay() => overlay != null && overlay.Enabled;

        private ThemePalette CurrentPalette()
        {
            var realm = context?.CurrentPlayer?.RealmIndex ?? 0;
            return RealmTheme.ForRealm(realm);
        }
    }
}
