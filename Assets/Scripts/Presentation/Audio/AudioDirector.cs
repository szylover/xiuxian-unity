// ============================================================
// AudioDirector.cs — maps game/animation beats onto audio cues
// ============================================================

using System;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Presentation.Animation;
using Xiuxian.Systems;

namespace Xiuxian.Presentation.Audio
{
    public sealed class AudioDirector : IDisposable
    {
        private readonly GameContext context;
        private readonly AudioManager audioManager;
        private readonly AnimationDirector animationDirector;

        public AudioDirector(GameContext context, AudioManager audioManager, AnimationDirector animationDirector = null)
        {
            this.context = context;
            this.audioManager = audioManager;
            this.animationDirector = animationDirector;
            if (context?.Bus != null) context.Bus.Subscribe(OnGameEvent);
            if (animationDirector != null)
            {
                animationDirector.BreakthroughSucceeded += OnBreakthroughSucceeded;
                animationDirector.BreakthroughFailed += OnBreakthroughFailed;
                animationDirector.RealmAdvanced += OnRealmAdvanced;
            }
            RefreshBgm();
        }

        public void Dispose()
        {
            if (context?.Bus != null) context.Bus.Unsubscribe(OnGameEvent);
            if (animationDirector != null)
            {
                animationDirector.BreakthroughSucceeded -= OnBreakthroughSucceeded;
                animationDirector.BreakthroughFailed -= OnBreakthroughFailed;
                animationDirector.RealmAdvanced -= OnRealmAdvanced;
            }
        }

        private void OnGameEvent(in GameEvent gameEvent)
        {
            switch (gameEvent.Type)
            {
                case GameEventType.CultivationChanged:
                    Play(SoundCue.CultivateTick);
                    break;
                case GameEventType.CombatChanged:
                    Play(SoundCue.CombatHit);
                    break;
                case GameEventType.BreakthroughChanged:
                    if (animationDirector == null) Play(SoundCue.BreakthroughFailure);
                    break;
                case GameEventType.RealmChanged:
                    if (animationDirector == null) Play(SoundCue.BreakthroughSuccess);
                    RefreshBgm();
                    break;
                case GameEventType.InventoryChanged:
                case GameEventType.CurrencyChanged:
                    Play(SoundCue.ItemGain);
                    break;
                case GameEventType.GameOver:
                    Play(SoundCue.Death);
                    break;
                case GameEventType.PlayerCreated:
                case GameEventType.PlayerLoaded:
                case GameEventType.DatabaseLoaded:
                case GameEventType.RegionChanged:
                case GameEventType.MapChanged:
                    RefreshBgm();
                    break;
            }
        }

        private void OnBreakthroughSucceeded(ThemePalette _) => Play(SoundCue.BreakthroughSuccess);
        private void OnBreakthroughFailed(ThemePalette _) => Play(SoundCue.BreakthroughFailure);
        private void OnRealmAdvanced(ThemePalette _) => RefreshBgm();

        private void Play(SoundCue cue)
        {
            if (audioManager == null) return;
            audioManager.PlayCue(cue);
        }

        private void RefreshBgm()
        {
            if (audioManager == null || context?.CurrentPlayer == null) return;
            var player = context.CurrentPlayer;
            var region = CurrentRegion();
            var regionId = region?.Id ?? "default";
            var element = region != null ? RealmTheme.GuessRegionElement(region) : RealmTheme.DominantElement(player);
            audioManager.PlayBgmFor(regionId, player.RealmIndex, element);
        }

        private RegionDef CurrentRegion()
        {
            try
            {
                return context?.Database == null || context.CurrentPlayer == null
                    ? null
                    : MapSystem.GetCurrentRegion(context.Database, context.CurrentPlayer);
            }
            catch
            {
                return null;
            }
        }
    }
}
