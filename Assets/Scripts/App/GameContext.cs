// ============================================================
// GameContext.cs — runtime app state shared by uGUI shell
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.App
{
    public sealed class GameContext
    {
        private readonly IDataSource dataSource;
        private readonly SystemRandomRng rng = new();

        public GameDatabase Database { get; private set; } = new();
        public SaveSystem SaveSystem { get; }
        public Player CurrentPlayer { get; private set; }
        public int CurrentSlot { get; private set; }
        public IReadOnlyList<string> AvailablePackIds { get; }
        public HashSet<string> SelectedPackIds { get; } = new();
        public List<string> LogEntries { get; } = new();
        public event Action<string, object> AppEvent;

        public GameContext(IDataSource dataSource, SaveSystem saveSystem)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            SaveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            AvailablePackIds = dataSource.ListPackages().OrderBy(PackageSortKey, StringComparer.Ordinal).ToArray();
            SelectedPackIds.Add("core");
            LoadDatabase(SelectedPackIds);
        }

        public PreviewRoll RollPreview() => PlayerFactory.RollPreview(rng);

        public void LoadDatabase(IEnumerable<string> packageIds)
        {
            var requested = new HashSet<string>(packageIds ?? Array.Empty<string>());
            requested.Add("core");
            Database = new GameDatabase();
            var loader = new DlcLoader(dataSource, Database);
            foreach (var id in AvailablePackIds.Where(id => requested.Contains(id)).OrderBy(PackageSortKey, StringComparer.Ordinal))
                loader.LoadPackage(id);
            SelectedPackIds.Clear();
            foreach (var id in Database.EnabledPackIds) SelectedPackIds.Add(id);
            Publish("database-loaded", Database);
        }

        public Player CreateNewPlayer(int slotIndex, string playerName, string gender, int appearance, PreviewRoll preview, IEnumerable<string> enabledPacks)
        {
            CurrentSlot = slotIndex;
            LoadDatabase(enabledPacks);
            CurrentPlayer = PlayerFactory.CreatePlayer(Database, new CreatePlayerOptions
            {
                Name = playerName,
                Gender = gender,
                Appearance = appearance,
                Preview = preview,
                EnabledDLCs = SelectedPackIds.ToArray(),
            }, rng);
            AddLog(UiTexts.LogNewGame(CurrentPlayer.Name));
            SaveCurrent();
            Publish("player-created", CurrentPlayer);
            return CurrentPlayer;
        }

        public bool LoadSlot(int slotIndex)
        {
            var player = SaveSystem.LoadSlot(slotIndex);
            if (player == null) return false;
            CurrentSlot = slotIndex;
            CurrentPlayer = player;
            LoadDatabase(player.EnabledDLCs.Count > 0 ? player.EnabledDLCs : new[] { "core" });
            AddLog(UiTexts.LogLoaded(player.Name));
            Publish("player-loaded", player);
            return true;
        }

        public void SaveCurrent()
        {
            if (CurrentPlayer == null) return;
            SaveSystem.SaveSlot(CurrentSlot, CurrentPlayer);
            AddLog(UiTexts.LogSaved(CurrentSlot + 1));
            Publish("player-saved", CurrentPlayer);
        }

        public void ExitToStart()
        {
            CurrentPlayer = null;
            LogEntries.Clear();
            Publish("exit-to-start", null);
        }

        public void AddLog(string message)
        {
            LogEntries.Add(message);
            Publish("log-added", message);
        }

        public void Publish(string eventName, object payload)
            => AppEvent?.Invoke(eventName, payload);

        private static string PackageSortKey(string id)
            => id == "core" ? string.Empty : id ?? string.Empty;
    }
}
