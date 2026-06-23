// ============================================================
// SaveSystem.cs — multi-slot save/load orchestration
// UnityEngine-free
// ============================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiuxian.Systems
{
    public sealed class SaveSystem
    {
        public const string SaveKey = "xiuxian_save";
        public const int SaveSlotCount = 5;
        public const int CurrentSaveVersion = 1;

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
        };

        internal static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(JsonSettings);

        private readonly ISaveStorage storage;
        private readonly Func<long> nowMillis;

        public SaveSystem(ISaveStorage storage, Func<long> nowMillis = null)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.nowMillis = nowMillis ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public static string GetSlotKey(int slotIndex)
        {
            ValidateSlot(slotIndex);
            return slotIndex == 0 ? SaveKey : SaveKey + "_" + slotIndex;
        }

        public void SaveSlot(int slotIndex, Player player)
        {
            ValidateSlot(slotIndex);
            if (player == null) throw new ArgumentNullException(nameof(player));
            var data = new SaveData
            {
                SaveVersion = CurrentSaveVersion,
                SavedAt = nowMillis(),
                Player = player,
                Systems = SaveSystemStates.Capture(player),
            };
            storage.WriteSlot(slotIndex, JsonConvert.SerializeObject(data, JsonSettings));
        }

        public Player LoadSlot(int slotIndex)
        {
            ValidateSlot(slotIndex);
            var raw = storage.ReadSlot(slotIndex);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                var data = JsonConvert.DeserializeObject<SaveData>(raw, JsonSettings);
                data = Migrate(data);
                if (data?.Player == null) return null;
                data.Systems?.ApplyTo(data.Player);
                return data.Player;
            }
            catch
            {
                return null;
            }
        }

        public IReadOnlyList<SaveSlotPreview> ListSlots()
        {
            var previews = new List<SaveSlotPreview>(SaveSlotCount);
            for (int i = 0; i < SaveSlotCount; i++) previews.Add(ReadPreview(i));
            return previews;
        }

        public void DeleteSlot(int slotIndex)
        {
            ValidateSlot(slotIndex);
            storage.DeleteSlot(slotIndex);
        }

        public string ExportSlot(int slotIndex)
        {
            ValidateSlot(slotIndex);
            return storage.ReadSlot(slotIndex);
        }

        public Player ImportSlot(int slotIndex, string json)
        {
            ValidateSlot(slotIndex);
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var data = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
                data = Migrate(data);
                if (data?.Player == null || string.IsNullOrWhiteSpace(data.Player.Name)) return null;
                storage.WriteSlot(slotIndex, json);
                return LoadSlot(slotIndex);
            }
            catch
            {
                return null;
            }
        }

        public static string ToCanonicalJson(Player player) => JsonConvert.SerializeObject(player, JsonSettings);

        private SaveSlotPreview ReadPreview(int slotIndex)
        {
            var empty = new SaveSlotPreview { SlotIndex = slotIndex, IsEmpty = true };
            var raw = storage.ReadSlot(slotIndex);
            if (string.IsNullOrWhiteSpace(raw)) return empty;
            try
            {
                var data = JsonConvert.DeserializeObject<SaveData>(raw, JsonSettings);
                data = Migrate(data);
                var p = data?.Player;
                if (p == null || string.IsNullOrWhiteSpace(p.Name)) return empty;
                return new SaveSlotPreview
                {
                    SlotIndex = slotIndex,
                    IsEmpty = false,
                    Name = p.Name,
                    RealmIndex = p.RealmIndex,
                    Age = p.Age,
                    GameYear = p.GameYear,
                    GameMonth = p.GameMonth,
                    SavedAt = data.SavedAt,
                };
            }
            catch
            {
                return empty;
            }
        }

        private static SaveData Migrate(SaveData data)
        {
            if (data == null) return null;
            data.Systems ??= new SaveSystemStates();
            if (data.SaveVersion <= 0) data.SaveVersion = CurrentSaveVersion;
            return data;
        }

        private static void ValidateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SaveSlotCount)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"slotIndex must be 0..{SaveSlotCount - 1}");
        }
    }
}
