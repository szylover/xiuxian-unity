// ============================================================
// GameEventBus.cs — UnityEngine-free typed state-to-UI event bus
// ============================================================

using System;
using System.Collections.Generic;

namespace Xiuxian.Core
{
    public enum GameEventType
    {
        DatabaseLoaded,
        PlayerCreated,
        PlayerLoaded,
        PlayerSaved,
        ExitToStart,
        LogAppended,
        ToastRequested,
        GameOver,
        TimeAdvanced,
        PlayerChanged,
        PlayerStatsChanged,
        InventoryChanged,
        EquipmentChanged,
        TechniquesChanged,
        DivineArtsChanged,
        RealmChanged,
        BreakthroughChanged,
        BodyCultivationChanged,
        CultivationChanged,
        CombatChanged,
        QuestChanged,
        MapChanged,
        RegionChanged,
        SectChanged,
        NpcChanged,
        DialogueChanged,
        CurrencyChanged,
        ShopChanged,
        AlchemyChanged,
        SmithingChanged,
        AuctionChanged,
        MiningChanged,
        BountyChanged,
        SecretRealmChanged,
        TalentChanged,
        EnlightenmentChanged,
        HeartDemonChanged,
        KarmaChanged,
        RankingChanged,
        PvpChanged,
        ReincarnationChanged,
        AscensionChanged,
        AchievementChanged,
        ChronicleChanged,
        DebugStateChanged,
    }

    public readonly struct GameEvent
    {
        public GameEvent(GameEventType type, object payload = null)
        {
            Type = type;
            Payload = payload;
        }

        public GameEventType Type { get; }
        public object Payload { get; }
    }

    public delegate void GameEventHandler(in GameEvent gameEvent);

    public sealed class GameEventBus
    {
        private readonly Dictionary<GameEventType, List<GameEventHandler>> typedHandlers = new();
        private readonly List<GameEventHandler> allHandlers = new();

        public void Subscribe(GameEventType type, GameEventHandler handler)
        {
            if (handler == null) return;
            if (!typedHandlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<GameEventHandler>();
                typedHandlers[type] = handlers;
            }
            handlers.Add(handler);
        }

        public void Subscribe(GameEventHandler handler)
        {
            if (handler == null) return;
            allHandlers.Add(handler);
        }

        public void Unsubscribe(GameEventType type, GameEventHandler handler)
        {
            if (handler == null) return;
            if (!typedHandlers.TryGetValue(type, out var handlers)) return;
            handlers.Remove(handler);
            if (handlers.Count == 0) typedHandlers.Remove(type);
        }

        public void Unsubscribe(GameEventHandler handler)
        {
            if (handler == null) return;
            allHandlers.Remove(handler);
        }

        public void Publish(GameEvent gameEvent)
        {
            if (typedHandlers.TryGetValue(gameEvent.Type, out var typed) && typed.Count > 0)
                DispatchSnapshot(typed, in gameEvent);
            if (allHandlers.Count > 0)
                DispatchSnapshot(allHandlers, in gameEvent);
        }

        public void Publish(GameEventType type, object payload = null)
            => Publish(new GameEvent(type, payload));

        private static void DispatchSnapshot(List<GameEventHandler> handlers, in GameEvent gameEvent)
        {
            var snapshot = handlers.ToArray();
            for (var i = 0; i < snapshot.Length; i++)
                snapshot[i]?.Invoke(in gameEvent);
        }
    }
}
