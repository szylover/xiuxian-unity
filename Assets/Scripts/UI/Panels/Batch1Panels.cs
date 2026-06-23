// ============================================================
// Batch1Panels.cs — issue #10 batch 1 concrete uGUI panels
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiuxian.App;
using Xiuxian.Core;
using Xiuxian.Data;
using Xiuxian.Systems;

namespace Xiuxian.UI
{
    public sealed class StatusPanel : PanelBase
    {
        public StatusPanel() : base(PanelId.Status, UiTexts.Status) { }

        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.RealmChanged || type == GameEventType.CultivationChanged || type == GameEventType.BodyCultivationChanged || type == GameEventType.InventoryChanged || type == GameEventType.EquipmentChanged || type == GameEventType.TimeAdvanced;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            AddRealm(content, p);
            AddBaseStats(content, p);
            AddCombatStats(content, p);
            AddInnate(content, p);
            AddAptitudes(content, p);
            AddTracking(content, p);
        }

        private void AddRealm(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionRealm);
            var realm = Context.Database.Realms.TryGetValue(p.RealmIndex, out var r) ? r.Name : UiTexts.RealmUnknown;
            var next = PlayerStatsSystem.GetNextRealm(Context.Database, p);
            UIBuilder.StatRow(card, UiTexts.QiRealm, UiTexts.Bracket(realm));
            UIBuilder.StatRow(card, UiTexts.QiExp, next == null ? p.Exp.ToString() : UiTexts.ExpText(p.Exp, next.ExpReq ?? 0));
            UIBuilder.ProgressBar(card, p.Exp, next?.ExpReq ?? Math.Max(1, p.Exp));
            UIBuilder.StatRow(card, UiTexts.Mp, UiTexts.CurrentMax(p.Mp, p.MaxMp));
            UIBuilder.StatRow(card, UiTexts.MentalPower, UiTexts.CurrentMax(p.MentalPower, p.MaxMentalPower));
            var body = Context.Database.BodyRealms.TryGetValue(p.BodyRealmIndex, out var b) ? b.Name : UiTexts.BodyRealm;
            var nextBody = BodyCultivationSystem.GetNextBodyRealm(Context.Database, p);
            UIBuilder.StatRow(card, UiTexts.BodyRealm, UiTexts.Bracket(body));
            UIBuilder.StatRow(card, UiTexts.BodyExp, nextBody == null ? p.BodyRealmExp.ToString() : UiTexts.ExpText(p.BodyRealmExp, nextBody.ExpReq ?? 0));
            UIBuilder.ProgressBar(card, p.BodyRealmExp, nextBody?.ExpReq ?? Math.Max(1, p.BodyRealmExp));
            UIBuilder.StatRow(card, UiTexts.Physique, UiTexts.CurrentMax(p.Physique, p.MaxPhysique));
            UIBuilder.StatRow(card, UiTexts.DamageReduce, UiTexts.Percent(p.PhysiqueDmgReduce));
            UIBuilder.StatRow(card, UiTexts.BodyTempering, p.BodyTempering.ToString());
        }

        private void AddBaseStats(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionBaseStats);
            UIBuilder.StatRow(card, UiTexts.Lifespan, UiTexts.StatValue(UiTexts.AgeMonths(p.Age), UiTexts.AgeYears(p.Lifespan)));
            UIBuilder.StatRow(card, UiTexts.Mood, UiTexts.CurrentMax(p.Mood, 100));
            UIBuilder.StatRow(card, UiTexts.Health, UiTexts.CurrentMax(p.Health, 100));
            UIBuilder.StatRow(card, UiTexts.Stamina, UiTexts.CurrentMax(p.Stamina, p.MaxStamina));
            UIBuilder.StatRow(card, UiTexts.Hp, UiTexts.CurrentMax(p.Hp, p.MaxHp));
            UIBuilder.StatRow(card, UiTexts.Gold, p.Gold.ToString());
            UIBuilder.StatRow(card, UiTexts.InventoryCapacity, UiTexts.CurrentMax(p.Inventory.Count, p.InventoryCapacity));
        }

        private static void AddCombatStats(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionCombatStats);
            UIBuilder.StatRow(card, UiTexts.Atk, p.Atk.ToString());
            UIBuilder.StatRow(card, UiTexts.Def, p.Def.ToString());
            UIBuilder.StatRow(card, UiTexts.Speed, p.Speed.ToString());
            UIBuilder.StatRow(card, UiTexts.MoveSpeed, p.MoveSpeed.ToString("0.#"));
            UIBuilder.StatRow(card, UiTexts.CritRate, UiTexts.Percent(p.CritRate));
            UIBuilder.StatRow(card, UiTexts.CritDmg, UiTexts.Multiplier(p.CritDmgMultiplier));
            UIBuilder.StatRow(card, UiTexts.CritResist, UiTexts.Percent(p.CritResist));
            UIBuilder.StatRow(card, UiTexts.SkillResist, UiTexts.Percent(p.SkillResist));
            UIBuilder.StatRow(card, UiTexts.SpellResist, UiTexts.Percent(p.SpellResist));
        }

        private static void AddInnate(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionInnate);
            UIBuilder.StatRow(card, UiTexts.Luck, UiTexts.CurrentMax(p.Luck, 100));
            UIBuilder.StatRow(card, UiTexts.Comprehension, UiTexts.CurrentMax(p.Comprehension, 100));
            UIBuilder.StatRow(card, UiTexts.Charisma, UiTexts.CurrentMax(p.Charisma, 100));
        }

        private static void AddAptitudes(Transform parent, Player p)
        {
            var roots = PlayerStatsSystem.GetSpiritRootDisplay(p.SpiritRoots);
            var card = PanelUi.Card(parent, UiTexts.StatValue(UiTexts.SectionSpiritRoots, UiTexts.Multiplier(roots.Multiplier)));
            UIBuilder.StatRow(card, UiTexts.SpiritRoot, UiTexts.RootSummary(p.SpiritRoots));
            AddApt(card, UiTexts.RootName("fire"), p.Aptitudes.Fire);
            AddApt(card, UiTexts.RootName("water"), p.Aptitudes.Water);
            AddApt(card, UiTexts.RootName("thunder"), p.Aptitudes.Thunder);
            AddApt(card, UiTexts.RootName("wind"), p.Aptitudes.Wind);
            AddApt(card, UiTexts.RootName("earth"), p.Aptitudes.Earth);
            AddApt(card, UiTexts.RootName("wood"), p.Aptitudes.Wood);
            AddApt(card, UiTexts.RootName("metal"), p.Aptitudes.Metal);
            card = PanelUi.Card(parent, UiTexts.SectionLifeAptitudes);
            AddApt(card, UiTexts.Alchemy, p.Aptitudes.Alchemy);
            AddApt(card, UiTexts.Smithing, p.Aptitudes.Smithing);
            AddApt(card, UiTexts.Mining, p.Aptitudes.Mining);
            card = PanelUi.Card(parent, UiTexts.SectionWeaponAptitudes);
            AddApt(card, UiTexts.TechniqueTypeName("sword"), p.Aptitudes.Sword);
            AddApt(card, UiTexts.TechniqueTypeName("blade"), p.Aptitudes.Blade);
            AddApt(card, UiTexts.TechniqueTypeName("spear"), p.Aptitudes.Spear);
            AddApt(card, UiTexts.TechniqueTypeName("fist"), p.Aptitudes.Fist);
            AddApt(card, UiTexts.TechniqueTypeName("palm"), p.Aptitudes.Palm);
            AddApt(card, UiTexts.TechniqueTypeName("finger"), p.Aptitudes.Finger);
        }

        private static void AddTracking(Transform parent, Player p)
        {
            var card = PanelUi.Card(parent, UiTexts.SectionTracking);
            UIBuilder.StatRow(card, UiTexts.Kills, p.Tracking.KillCount.ToString());
            UIBuilder.StatRow(card, UiTexts.BossKills, p.Tracking.BossKillCount.ToString());
        }

        private static void AddApt(Transform parent, string label, int value)
        {
            UIBuilder.StatRow(parent, label, UiTexts.CurrentMax(value, 100));
            UIBuilder.ProgressBar(parent, value, 100);
        }
    }

    public sealed class ActionPanel : PanelBase
    {
        public ActionPanel() : base(PanelId.Action, UiTexts.Action) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.CultivationChanged || type == GameEventType.BodyCultivationChanged || type == GameEventType.BreakthroughChanged || type == GameEventType.RealmChanged || type == GameEventType.TimeAdvanced;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            var actions = PanelUi.Card(content, UiTexts.Action);
            PanelUi.Button(actions, UiTexts.CultivateAction, Context.Cultivate, p.Stamina > 0);
            var bt = BreakthroughSystem.GetBreakthroughStatus(Context.Database, p);
            PanelUi.Button(actions, bt.NextRealm == null ? UiTexts.AlreadyPeak : UiTexts.BreakthroughTarget(bt.NextRealm.Name, bt.SuccessRate), Context.AttemptBreakthrough, bt.CanAttempt);
            PanelUi.Button(actions, UiTexts.BodyCultivateAction, Context.BodyCultivate, p.Stamina > 0);
            var body = BodyCultivationSystem.GetBodyBreakthroughStatus(Context.Database, p);
            PanelUi.Button(actions, body.NextRealm == null ? UiTexts.AlreadyPeak : UiTexts.BodyBreakthroughTarget(body.NextRealm.Name), Context.AttemptBodyBreakthrough, body.CanAttempt);
            PanelUi.Button(actions, UiTexts.RestAction, Context.Rest, true);

            var status = PanelUi.Card(content, UiTexts.Requirement);
            if (bt.NextRealm != null)
            {
                UIBuilder.StatRow(status, UiTexts.QiExp, UiTexts.ExpText(p.Exp, bt.NextRealm.ExpReq ?? 0));
                foreach (var item in bt.ItemsReady) UIBuilder.StatRow(status, item.Name, UiTexts.ExpText(item.Have, item.Required));
                foreach (var cond in bt.ConditionsReady) UIBuilder.StatRow(status, cond.Description, cond.Ready ? UiTexts.Unlocked : UiTexts.Locked);
            }
            if (body.NextRealm != null)
            {
                UIBuilder.StatRow(status, UiTexts.BodyExp, UiTexts.ExpText(p.BodyRealmExp, body.NextRealm.ExpReq ?? 0));
                UIBuilder.StatRow(status, UiTexts.Physique, UiTexts.ExpText(p.Physique, body.PhysiqueRequired));
            }
            foreach (var active in BottleneckSystem.GetActiveBottlenecks(Context.Database, p))
                UIBuilder.StatRow(status, UiTexts.ActiveBottleneck, active.Def.Name);
        }
    }

    public sealed class TalentPanel : PanelBase
    {
        public TalentPanel() : base(PanelId.Talent, UiTexts.Talent) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.TalentChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            if (string.IsNullOrEmpty(p.DestinyId)) DestinySystem.EnsureDestiny(p, new SystemRandomRng());
            PanelUi.Page(parent, Title, out var content);
            var state = DestinySystem.GetState(p);
            var destiny = DestinySystem.CoreDestinies.FirstOrDefault(d => d.Id == p.DestinyId);
            var card = PanelUi.Card(content, UiTexts.DestinyTitle);
            if (destiny == null) UIBuilder.StatRow(card, UiTexts.DestinyTitle, UiTexts.NoDestiny);
            else AddRuntimeDef(card, destiny.Name, destiny.Description, destiny.Effect);

            card = PanelUi.Card(content, UiTexts.TalentTitle);
            UIBuilder.StatRow(card, UiTexts.TalentPoints, state.TalentPoints.ToString());
            var talentIds = state.AcquiredTalentIds.Concat(p.TalentIds).Distinct().ToArray();
            if (talentIds.Length == 0) UIBuilder.StatRow(card, UiTexts.TalentTitle, UiTexts.NoTalents);
            foreach (var id in talentIds)
            {
                var talent = DestinySystem.CoreTalents.FirstOrDefault(t => t.Id == id);
                if (talent != null) AddRuntimeDef(card, talent.Name, talent.Description, talent.Effect);
            }

            card = PanelUi.Card(content, UiTexts.TalentTreeTitle);
            foreach (var node in DestinySystem.CoreTalentNodes.OrderBy(n => n.Tier).ThenBy(n => n.Id))
            {
                var talent = DestinySystem.CoreTalents.FirstOrDefault(t => t.Id == node.TalentId);
                var unlocked = state.UnlockedNodeIds.Contains(node.Id);
                var available = !unlocked && state.TalentPoints >= node.Cost && node.PrereqNodeIds.All(state.UnlockedNodeIds.Contains);
                var row = UIBuilder.Rect("TalentNode", card);
                UIBuilder.Horizontal(row, 4, 8).childAlignment = TextAnchor.MiddleLeft;
                UIBuilder.Layout(row, preferredHeight: 46);
                UIBuilder.Layout(UIBuilder.Label(row.transform, talent?.Name ?? node.TalentId, 21, TextAlignmentOptions.Left).gameObject, flexibleWidth: 1, preferredHeight: 42);
                var button = UIBuilder.Button(row.transform, unlocked ? UiTexts.Unlocked : available ? UiTexts.Unlock : UiTexts.Locked, () => Context.UnlockTalentNode(node.Id));
                button.interactable = available;
                UIBuilder.Layout(button.gameObject, preferredWidth: 120, preferredHeight: 42);
            }
        }

        private static void AddRuntimeDef(Transform parent, string name, string desc, StatEffect effect)
        {
            UIBuilder.SectionHeader(parent, string.IsNullOrEmpty(name) ? UiTexts.Unknown : name);
            if (!string.IsNullOrEmpty(desc)) PanelUi.Text(parent, desc, 20);
            foreach (var line in UiTexts.DescribeEffect(effect)) UIBuilder.StatRow(parent, UiTexts.EffectTitle, line);
        }
    }

    public sealed class TechniquePanel : PanelBase
    {
        public TechniquePanel() : base(PanelId.Technique, UiTexts.Technique) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.TechniquesChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            var card = PanelUi.Card(content, UiTexts.KnownTechniques);
            if (p.Techniques.Count == 0) PanelUi.Text(card, UiTexts.NoTechniques, 22);
            foreach (var slot in p.Techniques)
            {
                if (!Context.Database.Techniques.TryGetValue(slot.TechniqueId, out var def)) continue;
                var tech = PanelUi.Card(card, def.Name);
                UIBuilder.StatRow(tech, UiTexts.Level, UiTexts.LevelText(slot.Level, def.MaxLevel ?? slot.Level));
                UIBuilder.StatRow(tech, UiTexts.Category, UiTexts.TechniqueTypeName(def.Type));
                UIBuilder.StatRow(tech, UiTexts.EachLevel, UiTexts.FormatStats(def.StatBonusPerLevel));
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(tech, def.Description, 19);
                if (def.PassiveEffects != null && def.PassiveEffects.Count > 0)
                {
                    UIBuilder.SectionHeader(tech, UiTexts.PassiveEffects);
                    foreach (var passive in def.PassiveEffects)
                        UIBuilder.StatRow(tech, UiTexts.LevelText((int?)passive["minLevel"] ?? 0, def.MaxLevel ?? 1), (string)passive["description"] ?? UiTexts.FormatStats(new Dictionary<string, double> { { (string)passive["stat"] ?? string.Empty, (double?)passive["value"] ?? 0 } }));
                }
                var row = UIBuilder.Rect("TechniqueActions", tech);
                UIBuilder.Horizontal(row, 4, 8);
                var max = Math.Max(1, def.MaxLevel ?? 1);
                PanelUi.Button(row.transform, UiTexts.Practice, () => Context.PracticeTechnique(slot.TechniqueId), slot.Level < max);
                PanelUi.Button(row.transform, p.ActiveTechniqueId == slot.TechniqueId ? UiTexts.Deactivate : UiTexts.Activate, () => Context.SetActiveTechnique(slot.TechniqueId), true);
            }
        }
    }

    public sealed class DivineArtsPanel : PanelBase
    {
        public DivineArtsPanel() : base(PanelId.DivineArts, UiTexts.DivineArts) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.DivineArtsChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = DivineArtSystem.GetState(p);
            PanelUi.Page(parent, Title, out var content);
            var activeName = !string.IsNullOrEmpty(state.ActiveArtId) && Context.Database.DivineArts.TryGetValue(state.ActiveArtId, out var active) ? active.Name : UiTexts.None;
            UIBuilder.StatRow(content, UiTexts.ActiveDivineArt, activeName);
            var card = PanelUi.Card(content, UiTexts.SectionCount(UiTexts.LearnedDivineArts, state.Learned.Count));
            if (state.Learned.Count == 0) PanelUi.Text(card, UiTexts.NoDivineArts, 22);
            foreach (var learned in state.Learned)
            {
                if (!Context.Database.DivineArts.TryGetValue(learned.ArtId, out var def)) continue;
                var art = PanelUi.Card(card, def.Name);
                UIBuilder.StatRow(art, UiTexts.SpiritRoot, UiTexts.ElementName(def.Element));
                UIBuilder.StatRow(art, UiTexts.MpCost, (def.MpCost ?? 0).ToString());
                UIBuilder.StatRow(art, UiTexts.Cooldown, (def.Cooldown ?? 0).ToString());
                UIBuilder.StatRow(art, UiTexts.TriggerRate, UiTexts.Percent((def.TriggerRate ?? 0) * 100));
                UIBuilder.StatRow(art, UiTexts.Aptitude, UiTexts.Multiplier(DivineArtSystem.CalcAptitudePower(p, def)));
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(art, def.Description, 19);
                if (def.Effects != null) foreach (var effect in def.Effects) UIBuilder.StatRow(art, UiTexts.EffectTitle, FormatEffect(effect));
                PanelUi.Button(art, state.ActiveArtId == def.Id ? UiTexts.Deactivate : UiTexts.Activate, () => Context.ActivateDivineArt(def.Id), true);
            }
        }

        private static string FormatEffect(JToken token) => token == null ? UiTexts.None : token.ToString(Newtonsoft.Json.Formatting.None);
    }

    public sealed class EquipmentPanel : PanelBase
    {
        private static readonly string[] Slots = { "weapon", "helmet", "armor", "boots", "accessory1", "accessory2" };
        public EquipmentPanel() : base(PanelId.Equipment, UiTexts.Equipment) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.EquipmentChanged || type == GameEventType.InventoryChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            var equipped = PanelUi.Card(content, UiTexts.EquippedSlots);
            foreach (var slot in Slots)
            {
                var id = GetSlot(p.Equipped, slot);
                var name = !string.IsNullOrEmpty(id) && EquipmentSystem.GetEquipDef(Context.Database, p, id) is { } def ? def.Name : UiTexts.EmptySlotLabel;
                var row = UIBuilder.Rect("EquipSlot", equipped);
                UIBuilder.Horizontal(row, 4, 8).childAlignment = TextAnchor.MiddleLeft;
                UIBuilder.Layout(row, preferredHeight: 46);
                UIBuilder.Layout(UIBuilder.Label(row.transform, UiTexts.EquipmentSlotName(slot), 21, TextAlignmentOptions.Left).gameObject, preferredWidth: 130, preferredHeight: 42);
                UIBuilder.Layout(UIBuilder.Label(row.transform, name, 21, TextAlignmentOptions.Left).gameObject, flexibleWidth: 1, preferredHeight: 42);
                PanelUi.Button(row.transform, UiTexts.Unequip, () => Context.UnequipItem(slot), !string.IsNullOrEmpty(id), 100);
            }
            var bag = PanelUi.Card(content, UiTexts.EquippableItems);
            foreach (var inv in p.Inventory)
            {
                var def = EquipmentSystem.GetEquipDef(Context.Database, p, inv.ItemId);
                if (def == null) continue;
                var item = PanelUi.Card(bag, def.Name);
                UIBuilder.StatRow(item, UiTexts.Count, inv.Count.ToString());
                UIBuilder.StatRow(item, UiTexts.Category, UiTexts.EquipmentSlotName(def.Slot));
                UIBuilder.StatRow(item, UiTexts.Rarity, UiTexts.RarityName(def.Rarity));
                UIBuilder.StatRow(item, UiTexts.Stats, UiTexts.FormatStats(def.Stats));
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(item, def.Description, 18);
                PanelUi.Button(item, UiTexts.Equip, () => Context.EquipItem(inv.ItemId), p.RealmIndex >= (def.MinRealm ?? 0));
            }
        }

        private static string GetSlot(EquippedSlots slots, string slot) => slot switch { "weapon" => slots.Weapon, "helmet" => slots.Helmet, "armor" => slots.Armor, "boots" => slots.Boots, "accessory1" => slots.Accessory1, "accessory2" => slots.Accessory2, _ => null };
    }

    public sealed class InventoryPanel : PanelBase
    {
        public InventoryPanel() : base(PanelId.Inventory, UiTexts.Inventory) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.InventoryChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.CurrencyChanged;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            PanelUi.Page(parent, Title, out var content);
            UIBuilder.StatRow(content, UiTexts.InventoryCapacity, UiTexts.CurrentMax(p.Inventory.Count, p.InventoryCapacity));
            UIBuilder.ProgressBar(content, p.Inventory.Count, p.InventoryCapacity);
            if (p.Inventory.Count == 0) PanelUi.Text(content, UiTexts.InventoryEmpty, 24);
            foreach (var slot in p.Inventory)
            {
                Context.Database.Items.TryGetValue(slot.ItemId, out var def);
                var card = PanelUi.Card(content, def?.Name ?? slot.ItemId);
                UIBuilder.StatRow(card, UiTexts.Count, slot.Count.ToString());
                UIBuilder.StatRow(card, UiTexts.Category, UiTexts.CategoryName(def?.Category));
                UIBuilder.StatRow(card, UiTexts.Rarity, UiTexts.RarityName(def?.Rarity));
                if (!string.IsNullOrEmpty(def?.Description)) PanelUi.Text(card, def.Description, 18);
                if (def?.Effects != null) UIBuilder.StatRow(card, UiTexts.EffectTitle, UiTexts.FormatStats(def.Effects.ToDictionary(kv => kv.Key, kv => kv.Value.Kind == EffectValueKind.Scalar ? kv.Value.Scalar : 0)));
                var row = UIBuilder.Rect("InventoryActions", card);
                UIBuilder.Horizontal(row, 4, 8);
                PanelUi.Button(row.transform, def != null && (def.Usable || def.Category == "scroll") ? UiTexts.Use : UiTexts.Usable, () => Context.UseItem(slot.ItemId), def != null && (def.Usable || def.Category == "scroll"));
                PanelUi.Button(row.transform, UiTexts.Drop, () => Context.DropItem(slot.ItemId), true);
            }
        }
    }

    public sealed class EnlightenmentPanel : PanelBase
    {
        public EnlightenmentPanel() : base(PanelId.Enlightenment, UiTexts.Enlightenment) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.EnlightenmentChanged || type == GameEventType.PlayerChanged || type == GameEventType.PlayerStatsChanged || type == GameEventType.TimeAdvanced;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = EnlightenmentSystem.GetState(p);
            PanelUi.Page(parent, Title, out var content);
            var progress = PanelUi.Card(content, UiTexts.ProgressTitle);
            UIBuilder.StatRow(progress, UiTexts.Karma, UiTexts.AlignmentName(KarmaSystem.GetAlignment(p.Karma)));
            UIBuilder.StatRow(progress, UiTexts.ComprehensionExp, UiTexts.ExpText(state.ComprehensionExp, EnlightenmentSystem.EnlightenmentPointExp));
            UIBuilder.ProgressBar(progress, state.ComprehensionExp, EnlightenmentSystem.EnlightenmentPointExp);
            UIBuilder.StatRow(progress, UiTexts.InsightPoints, state.InsightPoints.ToString());
            var row = UIBuilder.Rect("EnlightenmentActions", progress);
            UIBuilder.Horizontal(row, 4, 8);
            PanelUi.Button(row.transform, UiTexts.Contemplate, Context.ContemplateInsight, state.InsightPoints >= EnlightenmentSystem.ContemplateCost);
            PanelUi.Button(row.transform, UiTexts.TriggerInsight, Context.TriggerEnlightenment, true);

            var buffs = PanelUi.Card(content, UiTexts.ActiveBuffs);
            if (state.ActiveBuffs.Count == 0) PanelUi.Text(buffs, UiTexts.NoBuffs, 21);
            foreach (var buff in state.ActiveBuffs)
            {
                UIBuilder.SectionHeader(buffs, buff.Name);
                UIBuilder.StatRow(buffs, UiTexts.Remaining, UiTexts.RemainingMonths(buff.RemainingMonths));
                foreach (var line in UiTexts.DescribeEffect(buff.Effect)) UIBuilder.StatRow(buffs, UiTexts.EffectTitle, line);
            }

            var insights = PanelUi.Card(content, UiTexts.Insights);
            foreach (var def in EnlightenmentSystem.Insights)
            {
                var unlocked = state.UnlockedInsightIds.Contains(def.Id);
                UIBuilder.SectionHeader(insights, def.Name);
                UIBuilder.StatRow(insights, UiTexts.Category, UiTexts.AlignmentName(def.Route));
                UIBuilder.StatRow(insights, UiTexts.Status, unlocked ? UiTexts.Unlocked : UiTexts.Locked);
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(insights, def.Description, 18);
                foreach (var line in UiTexts.DescribeEffect(def.Effect)) UIBuilder.StatRow(insights, UiTexts.EffectTitle, line);
            }
        }
    }

    public sealed class LearningPanel : PanelBase
    {
        public LearningPanel() : base(PanelId.Learning, UiTexts.Learning) { }
        protected override bool ShouldRefreshOn(GameEventType type) => type == GameEventType.TechniquesChanged || type == GameEventType.DivineArtsChanged || type == GameEventType.InventoryChanged || type == GameEventType.PlayerChanged || type == GameEventType.TimeAdvanced;

        protected override void BuildContent(Transform parent)
        {
            var p = Context.CurrentPlayer;
            var state = LearningSystem.GetState(p);
            PanelUi.Page(parent, Title, out var content);
            var active = PanelUi.Card(content, UiTexts.CurrentStudy);
            if (state.ActiveStudy == null) PanelUi.Text(active, UiTexts.NoActiveStudy, 21);
            else
            {
                UIBuilder.SectionHeader(active, UiTexts.StudyTarget(GetStudyTargetName(Context.Database, state.ActiveStudy)));
                UIBuilder.StatRow(active, UiTexts.ProgressTitle, UiTexts.ProgressMonths(state.ActiveStudy.ProgressMonths, state.ActiveStudy.TotalMonths));
                UIBuilder.ProgressBar(active, state.ActiveStudy.ProgressMonths, state.ActiveStudy.TotalMonths);
                var row = UIBuilder.Rect("StudyActions", active);
                UIBuilder.Horizontal(row, 4, 8);
                PanelUi.Button(row.transform, UiTexts.AdvanceStudy, Context.TickStudy, true);
                PanelUi.Button(row.transform, UiTexts.CancelStudy, Context.CancelStudy, true);
            }

            var scrolls = PanelUi.Card(content, UiTexts.ScrollList);
            var found = false;
            foreach (var inv in p.Inventory)
            {
                if (!Context.Database.Items.TryGetValue(inv.ItemId, out var def) || def.Category != "scroll") continue;
                found = true;
                var item = PanelUi.Card(scrolls, def.Name);
                UIBuilder.StatRow(item, UiTexts.Count, inv.Count.ToString());
                UIBuilder.StatRow(item, UiTexts.Category, UiTexts.ScrollTypeName(def.ScrollType));
                UIBuilder.StatRow(item, UiTexts.Rarity, UiTexts.RarityName(def.Rarity));
                UIBuilder.StatRow(item, UiTexts.Remaining, UiTexts.RemainingMonths(LearningSystem.CalcActualStudyMonths(def.ScrollStudyMonths ?? 1, p.Comprehension)));
                UIBuilder.StatRow(item, UiTexts.Technique, GetScrollTargetName(Context.Database, def));
                if (!string.IsNullOrEmpty(def.Description)) PanelUi.Text(item, def.Description, 18);
                PanelUi.Button(item, UiTexts.StartStudy, () => Context.StartStudy(inv.ItemId), state.ActiveStudy == null);
            }
            if (!found) PanelUi.Text(scrolls, UiTexts.NoScrolls, 21);
        }

        private static string GetStudyTargetName(GameDatabase db, ActiveStudy study)
        {
            if (study == null) return UiTexts.Unknown;
            return GetTargetName(db, study.ScrollType, study.TargetId);
        }

        private static string GetScrollTargetName(GameDatabase db, ItemDef scroll)
            => GetTargetName(db, scroll.ScrollType, scroll.ScrollTargetId);

        private static string GetTargetName(GameDatabase db, string type, string id)
        {
            if (type == "technique" && db.Techniques.TryGetValue(id, out var tech)) return tech.Name;
            if (type == "divineArt" && db.DivineArts.TryGetValue(id, out var art)) return art.Name;
            if (type == "recipe" && db.Recipes.TryGetValue(id, out var recipe)) return recipe.Name;
            if (type == "smithingRecipe" && db.SmithingRecipes.TryGetValue(id, out var smith)) return smith.Name;
            return id ?? UiTexts.Unknown;
        }
    }

    internal static class PanelUi
    {
        public static void Page(Transform parent, string title, out Transform content)
        {
            UIBuilder.Vertical(parent.gameObject, 18, 12);
            UIBuilder.Layout(UIBuilder.Label(parent, title, 40).gameObject, preferredHeight: 62);
            var scroll = UIBuilder.ScrollList(parent, title + "Scroll");
            UIBuilder.Layout(scroll.transform.parent.gameObject, flexibleHeight: 1);
            content = scroll.transform;
        }

        public static Transform Card(Transform parent, string title)
        {
            var card = UIBuilder.Card(parent, "Card").transform;
            UIBuilder.SectionHeader(card, title);
            return card;
        }

        public static void Text(Transform parent, string text, int size)
        {
            var label = UIBuilder.Label(parent, text ?? string.Empty, size, TextAlignmentOptions.TopLeft);
            UIBuilder.Layout(label.gameObject, preferredHeight: Math.Max(34, size * 3));
        }

        public static Button Button(Transform parent, string text, Action action, bool enabled, float width = -1)
        {
            var button = UIBuilder.Button(parent, text, action);
            button.interactable = enabled;
            UIBuilder.Layout(button.gameObject, preferredWidth: width, preferredHeight: 44, flexibleWidth: width < 0 ? 1 : -1);
            return button;
        }
    }
}
