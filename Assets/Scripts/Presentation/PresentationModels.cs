// ============================================================
// PresentationModels.cs — descriptors shared by presentation providers/views
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.Presentation
{
    public readonly struct ThemePalette
    {
        public ThemePalette(Color primary, Color secondary, Color accent, Color shadow)
        {
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Shadow = shadow;
        }

        public Color Primary { get; }
        public Color Secondary { get; }
        public Color Accent { get; }
        public Color Shadow { get; }
    }

    public readonly struct PortraitRequest
    {
        public PortraitRequest(Player player)
        {
            Player = player;
            Gender = PresentationAssetPaths.Normalize(player?.Gender);
            Appearance = player?.Appearance ?? 0;
            RealmIndex = player?.RealmIndex ?? 0;
        }

        public Player Player { get; }
        public string Gender { get; }
        public int Appearance { get; }
        public int RealmIndex { get; }
    }

    public sealed class PortraitDescriptor
    {
        public Sprite Sprite;
        public Sprite RealmOverlay;
        public ThemePalette Palette;
        public string AssetKey;
        public bool Procedural;
    }

    public readonly struct SceneRequest
    {
        public SceneRequest(GameDatabase database, Player player, RegionDef region, IReadOnlyList<SceneExit> exits, IReadOnlyList<NpcDef> npcs)
        {
            Database = database;
            Player = player;
            Region = region;
            Exits = exits ?? Array.Empty<SceneExit>();
            Npcs = npcs ?? Array.Empty<NpcDef>();
            RealmIndex = player?.RealmIndex ?? 0;
        }

        public GameDatabase Database { get; }
        public Player Player { get; }
        public RegionDef Region { get; }
        public IReadOnlyList<SceneExit> Exits { get; }
        public IReadOnlyList<NpcDef> Npcs { get; }
        public int RealmIndex { get; }
        public string RegionId => Region?.Id ?? "default";
    }

    public sealed class SceneDescriptor
    {
        public Sprite Background;
        public ThemePalette Palette;
        public string AssetKey;
        public bool Procedural;
        public string Title;
        public string Description;
        public IReadOnlyList<SceneExit> Exits = Array.Empty<SceneExit>();
        public IReadOnlyList<NpcDef> Npcs = Array.Empty<NpcDef>();
    }

    public sealed class PortraitChangedEventArgs : EventArgs
    {
        public PortraitChangedEventArgs(PortraitDescriptor portrait) => Portrait = portrait;
        public PortraitDescriptor Portrait { get; }
    }

    public sealed class SceneChangedEventArgs : EventArgs
    {
        public SceneChangedEventArgs(SceneDescriptor scene) => Scene = scene;
        public SceneDescriptor Scene { get; }
    }
}
