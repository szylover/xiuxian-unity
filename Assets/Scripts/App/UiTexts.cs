// ============================================================
// UiTexts.cs — centralized Chinese UI copy for the uGUI shell
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xiuxian.Systems;

namespace Xiuxian.App
{
    public static class UiTexts
    {
        public const string GameTitle = "修仙之路";
        public const string Subtitle = "踏入修仙世界，逆天改命";
        public const string NewGame = "新入仙途";
        public const string ContinueGame = "读取存档";
        public const string Quit = "退出";
        public const string Back = "返回";
        public const string Confirm = "确认";
        public const string EmptySlot = "空存档";
        public const string Slot = "存档槽";
        public const string CharacterCreation = "创建角色";
        public const string DaoName = "道号";
        public const string NamePlaceholder = "留空则为无名散修";
        public const string Male = "男修";
        public const string Female = "女修";
        public const string Appearance = "外貌";
        public const string Reroll = "随机属性";
        public const string StartCultivation = "开始修炼";
        public const string DlcSelection = "内容包选择";
        public const string CoreDlcLocked = "基础包（必选）";
        public const string MainMenu = "主菜单";
        public const string Status = "状态";
        public const string GameLog = "修行日志";
        public const string Save = "保存";
        public const string SaveNow = "保存当前进度";
        public const string PlaceholderTitle = "功能占位";
        public const string PlaceholderBody = "完整面板将在后续 UI 任务中接入。";
        public const string GameOverTitle = "道消身陨";
        public const string Restart = "重新开始";
        public const string LoadingFailed = "数据加载失败";
        public const string NoLog = "暂无日志";
        public const string RealmUnknown = "未知境界";
        public const string Unknown = "未知";
        public const string None = "无";
        public const string AgeUnit = "岁";
        public const string MonthUnit = "月";
        public const string Hp = "体力";
        public const string Mp = "灵力";
        public const string Stamina = "精力";
        public const string MentalPower = "念力";
        public const string Physique = "体魄";
        public const string Gold = "灵石";
        public const string Age = "年龄";
        public const string Lifespan = "寿元";
        public const string Luck = "运气";
        public const string Karma = "业力";
        public const string Comprehension = "悟性";
        public const string Charisma = "魅力";
        public const string Mood = "心境";
        public const string Health = "健康";
        public const string SpiritRoot = "灵根";
        public const string Aptitude = "资质";
        public const string Cultivation = "修炼";
        public const string Action = "行动";
        public const string Inventory = "背包";
        public const string Map = "地图";
        public const string Combat = "战斗";
        public const string Sect = "门派";
        public const string Quests = "任务";
        public const string Quest = "任务";
        public const string Shop = "商店";
        public const string Equipment = "装备";
        public const string Technique = "功法";
        public const string DivineArts = "神通";
        public const string Talent = "天赋";
        public const string Enlightenment = "悟道";
        public const string Learning = "学习";
        public const string Alchemy = "炼丹";
        public const string Smithing = "炼器";
        public const string Crafting = "制作";
        public const string Auction = "拍卖";
        public const string Mining = "采矿";
        public const string World = "世界";
        public const string Npc = "NPC";
        public const string SecretRealm = "秘境";
        public const string Bounty = "悬赏";
        public const string Companion = "同伴";
        public const string Achievement = "成就";
        public const string Chronicle = "履历";
        public const string Ranking = "排行";
        public const string Pvp = "论道";
        public const string HeartDemon = "心魔";
        public const string NavCategoryCultivation = "角色 / 修炼";
        public const string NavCategoryEconomy = "物品 / 经济";
        public const string NavCategoryWorld = "世界 / 社交";
        public const string NavCategoryAdvanced = "进阶";
        public const string NavCategorySystem = "系统";
        public const string SectionRealm = "修炼境界";
        public const string SectionBaseStats = "基础属性";
        public const string SectionCombatStats = "战斗属性";
        public const string SectionInnate = "先天属性";
        public const string SectionSpiritRoots = "灵根资质";
        public const string SectionLifeAptitudes = "生活资质";
        public const string SectionWeaponAptitudes = "武技资质";
        public const string SectionTracking = "追踪数据";
        public const string QiRealm = "气修境界";
        public const string QiExp = "气修修为";
        public const string BodyRealm = "体修境界";
        public const string BodyExp = "体修修为";
        public const string BodyTempering = "淬体次数";
        public const string DamageReduce = "体魄减伤";
        public const string InventoryCapacity = "背包容量";
        public const string Atk = "攻击";
        public const string Def = "防御";
        public const string Speed = "脚力";
        public const string MoveSpeed = "移速";
        public const string CritRate = "会心率";
        public const string CritDmg = "会心伤害";
        public const string CritResist = "护心";
        public const string SkillResist = "功法抗性";
        public const string SpellResist = "神通抗性";
        public const string Kills = "击杀数";
        public const string BossKills = "Boss 击杀";
        public const string CultivateAction = "修炼 / 打坐";
        public const string BreakthroughAction = "气修突破";
        public const string BodyCultivateAction = "体修淬炼";
        public const string BodyBreakthroughAction = "体修突破";
        public const string RestAction = "休息调息";
        public const string ActiveBottleneck = "瓶颈";
        public const string NeedMoreExp = "修为不足";
        public const string NeedMorePhysique = "体魄不足";
        public const string AlreadyPeak = "已达当前巅峰";
        public const string DestinyTitle = "命格";
        public const string TalentTitle = "天赋";
        public const string TalentTreeTitle = "天赋树";
        public const string TalentPoints = "天赋点";
        public const string Unlock = "解锁";
        public const string Unlocked = "已解锁";
        public const string Locked = "未解锁";
        public const string NoDestiny = "尚未觉醒命格";
        public const string NoTalents = "尚未获得天赋";
        public const string EffectTitle = "效果";
        public const string KnownTechniques = "已学功法";
        public const string NoTechniques = "尚未习得任何功法。可在学习面板研读功法卷轴。";
        public const string Practice = "修炼";
        public const string Activate = "激活";
        public const string Deactivate = "取消激活";
        public const string Active = "已激活";
        public const string Level = "等级";
        public const string EachLevel = "每级";
        public const string PassiveEffects = "熟练被动";
        public const string LearnedDivineArts = "已学神通";
        public const string ActiveDivineArt = "当前神通";
        public const string NoDivineArts = "尚未习得任何神通。可在学习面板研读神通卷轴。";
        public const string MpCost = "耗灵";
        public const string Cooldown = "冷却";
        public const string TriggerRate = "触发率";
        public const string EquippedSlots = "已装备";
        public const string EquippableItems = "可装备";
        public const string Unequip = "卸下";
        public const string Equip = "装备";
        public const string EmptySlotLabel = "空槽位";
        public const string InventoryEmpty = "空空如也。";
        public const string Use = "使用";
        public const string Drop = "丢弃";
        public const string Usable = "可使用";
        public const string ProgressTitle = "参悟进度";
        public const string InsightPoints = "悟道点";
        public const string ComprehensionExp = "悟性经验";
        public const string Contemplate = "参悟道痕";
        public const string TriggerInsight = "强行顿悟";
        public const string ActiveBuffs = "当前感悟";
        public const string NoBuffs = "暂无感悟加持";
        public const string Insights = "道痕";
        public const string CurrentStudy = "当前研读";
        public const string ScrollList = "卷轴列表";
        public const string NoActiveStudy = "当前没有研读任务";
        public const string NoScrolls = "背包中没有可研读卷轴";
        public const string StartStudy = "开始研读";
        public const string AdvanceStudy = "研读一月";
        public const string CancelStudy = "放弃研读";
        public const string Remaining = "剩余";
        public const string Count = "数量";
        public const string Category = "类别";
        public const string Rarity = "品阶";
        public const string Description = "描述";
        public const string Stats = "属性";
        public const string Requirement = "需求";
        public const string LogCultivate = "静坐吐纳，修为有所精进。";
        public const string LogRest = "调息休整，精力与体魄渐复。";
        public const string LogBodyCultivate = "搬运气血，体修修为有所增长。";
        public const string LogStudyTick = "潜心研读，卷轴进度推进一月。";
        public const string LogStudyCancel = "已放弃当前研读。";
        public const string LogTechniqueActivated = "功法已切换。";
        public const string LogDivineArtActivated = "神通已切换。";
        public const string LogItemUsed = "物品已使用。";
        public const string LogItemDropped = "物品已丢弃。";
        public const string OperationFailed = "操作未生效。";
        public const string NoAvailableRecipes = "暂无可用配方。";
        public const string NoAvailableSmithingRecipes = "暂无可用炼器配方。";
        public const string LearnedRecipeHint = "可在学习面板研读丹方或图谱来解锁更多配方。";
        public const string RecipeList = "配方列表";
        public const string SmithingRecipeList = "炼器图谱";
        public const string Materials = "材料";
        public const string Output = "产物";
        public const string SuccessRate = "成功率";
        public const string MentalCost = "念力消耗";
        public const string GoldCost = "灵石消耗";
        public const string BrewAlchemy = "开炉炼丹";
        public const string ForgeSmithing = "开炉炼器";
        public const string Buy = "买入";
        public const string Sell = "卖出";
        public const string ShopBuyList = "商店货架";
        public const string ShopSellList = "背包可售";
        public const string MerchantAbsent = "商人还没到。";
        public const string EmptyBagShort = "背包空空。";
        public const string UnitPrice = "单价";
        public const string OriginalPrice = "原价";
        public const string SellPrice = "卖价";
        public const string Stock = "库存";
        public const string AuctionIntro = "竞拍珍宝或寄售物品，拍卖会每隔数月刷新。";
        public const string AuctionLots = "当前拍品";
        public const string AuctionConsignments = "寄售中";
        public const string AuctionHistory = "拍卖记录";
        public const string EmptyAuctionLots = "暂无拍品。";
        public const string EmptyAuctionConsignments = "暂无寄售。";
        public const string EmptyAuctionHistory = "暂无记录。";
        public const string RefreshAuction = "刷新拍卖";
        public const string SettleAuction = "结算拍卖";
        public const string Bid = "出价";
        public const string Consign = "寄售";
        public const string BasePrice = "底价";
        public const string CurrentBid = "当前价";
        public const string HighestBidder = "最高出价";
        public const string TimeLeft = "剩余";
        public const string AskPrice = "标价";
        public const string PlayerBidder = "你";
        public const string NoBidder = "无人";
        public const string MiningIntro = "寻访灵脉，消耗精力与时间采集矿材。";
        public const string MiningSummary = "采矿总览";
        public const string AvailableMiningSites = "可采矿脉";
        public const string EmptyMiningSites = "当前区域暂无可采矿脉。";
        public const string Mine = "采矿";
        public const string FengShui = "风水";
        public const string MinRealm = "最低境界";
        public const string Cost = "消耗";
        public const string LastMiningSite = "上次矿脉";
        public const string CurrentRegion = "当前位置";
        public const string WorldRegions = "世界区域";
        public const string Travel = "前往";
        public const string Here = "您在这里";
        public const string SafeZone = "安全区";
        public const string LockedReason = "未解锁";
        public const string RegionTags = "区域标签";
        public const string ActiveQuests = "进行中";
        public const string DiscoveredQuests = "已发现";
        public const string CompletedQuests = "已完成";
        public const string TrackedQuest = "追踪任务";
        public const string NoActiveQuests = "暂无进行中的任务。";
        public const string NoDiscoveredQuests = "暂无新任务。";
        public const string NoCompletedQuests = "尚未完成任务。";
        public const string AcceptQuest = "接受";
        public const string TrackQuest = "追踪";
        public const string UntrackQuest = "取消追踪";
        public const string DeliverItem = "交付";
        public const string TurnInQuest = "领取奖励";
        public const string PendingTurnIn = "待交付";
        public const string Objectives = "目标";
        public const string Rewards = "奖励";
        public const string Step = "阶段";
        public const string CompletedAt = "完成时间";
        public const string RegionNpcs = "当前区域";
        public const string Contacts = "人脉总览";
        public const string NoRegionNpcs = "当前区域无 NPC。";
        public const string NoContacts = "尚未邂逅任何人。";
        public const string MeetNpc = "邂逅";
        public const string ChatNpc = "攀谈";
        public const string GiftNpc = "赠礼";
        public const string Relation = "关系";
        public const string Affinity = "好感";
        public const string Role = "身份";
        public const string Personality = "性情";
        public const string Disposition = "立场";
        public const string HomeRegion = "故乡";
        public const string QuestsAtNpc = "相关任务";
        public const string Available = "可接";
        public const string CanTurnIn = "可交付";

        public static string SlotTitle(int slot) => $"{Slot} {slot}";
        public static string SlotPreview(SaveSlotPreview p) => p == null || p.IsEmpty ? EmptySlot : $"{p.Name}｜第{p.GameYear}年{p.GameMonth}月｜境界 {p.RealmIndex}";
        public static string DlcCount(int enabled, int total) => $"已选 {enabled}/{total}";
        public static string PackLabel(string id) => id == "core" ? CoreDlcLocked : id;
        public static string LogNewGame(string name) => $"{name} 踏上修仙之路。";
        public static string LogLoaded(string name) => $"已读取 {name} 的存档。";
        public static string LogSaved(int slot) => $"已保存至存档槽 {slot}。";
        public static string PanelPlaceholder(string title) => $"{title}：{PlaceholderBody}";
        public static string AgeYears(int months) => $"{months / 12}{AgeUnit}";
        public static string AgeMonths(int months) => $"{months / 12}{AgeUnit}{months % 12}{MonthUnit}";
        public static string RootSummary(PlayerSpiritRoots roots) => roots == null || roots.Roots.Count == 0 ? "无灵根" : string.Join("、", roots.Roots.ConvertAll(r => RootName(r.Type) + r.Affinity));
        public static string RootName(string type) => type switch { "metal" => "金", "wood" => "木", "water" => "水", "fire" => "火", "earth" => "土", "thunder" => "雷", "wind" => "风", _ => type };
        public static string ElementName(string type) => RootName(type);
        public static string GenderLabel(string gender) => gender == "female" ? Female : Male;
        public static string HudNameRealm(string name, string realm) => $"{name}【{realm}】";
        public static string StatValue(string label, object value) => $"{label} {value}";
        public static string StatCurrentMax(string label, int current, int max) => $"{label} {current}/{max}";
        public static string CurrentMax(int current, int max) => $"{current}/{max}";
        public static string Percent(double value) => $"{value:0.#}%";
        public static string Multiplier(double value) => $"×{value:0.##}";
        public static string Bracket(string value) => $"【{value}】";
        public static string RealmNeed(string realm) => $"需 {realm}";
        public static string LevelText(int level, int max) => $"Lv.{level}/{max}";
        public static string ExpText(int exp, int req) => $"{exp}/{req}";
        public static string CostText(string label, int value) => $"{label}：{value}";
        public static string CountText(int count) => $"×{count}";
        public static string Points(int points) => $"{TalentPoints}：{points}";
        public static string InsightPointText(int points) => $"{InsightPoints}：{points}";
        public static string ProgressMonths(int current, int total) => $"{current}/{total} 月";
        public static string RemainingMonths(int months) => $"{Remaining} {months} 月";
        public static string StudyTarget(string target) => $"《{target}》";
        public static string LearnedCount(int learned, int total) => $"{learned}/{total}";
        public static string ActiveName(string name) => $"{Active}：{name}";
        public static string CultivateGain(int gain) => $"修为 +{gain}";
        public static string BodyGain(int gain) => $"体修修为 +{gain}";
        public static string RecipeOutput(string name, int count) => $"→ {name} ×{count}";
        public static string ItemCount(string name, int count) => $"{name} ×{count}";
        public static string HaveNeed(int have, int need) => $"{have}/{need}";
        public static string TimeLeftMonths(int months) => $"{TimeLeft} {months} 月";
        public static string TravelCost(int stamina, int months) => $"{stamina} 精力 / {months} 月";
        public static string MiningTotal(int count, int fengShui) => $"已采 {count} 次，累计风水 {fengShui}";
        public static string SectionCountLabel(string label, int count) => $"{label}（{count}）";
        public static string QuestPendingTurnIn(string npcName) => $"请前往 {npcName} 交付任务。";
        public static string CompletedAtMonths(int months) => $"第 {months / 12 + 1} 年 {months % 12 + 1} 月";
        public static string RegionMinRealm(string qi, string body) => string.IsNullOrEmpty(body) ? $"需 {qi}" : $"需 {qi} / {body}";
        public static string LogOperation(string panel, string message) => $"{panel}：{message}";
        public static string RelationName(string level) => level switch { "hostile" => "敌对", "cold" => "冷淡", "stranger" => "陌生", "acquaintance" => "相识", "friend" => "好友", "close_friend" => "挚友", "soulmate" => "知己", _ => level ?? Unknown };
        public static string FengShuiGrade(string grade) => grade switch { "excellent" => "极佳", "good" => "上佳", "normal" => "平稳", "poor" => "贫瘠", "bad" => "凶煞", _ => grade ?? Unknown };
        public static string WorldActionMessage(string key) => key switch
        {
            "regionNotFound" => "区域不存在",
            "containerRegion" => "此处为区域分类，无法直接前往",
            "alreadyHere" => "已在此地",
            "realmInsufficient" => "境界不足",
            "staminaInsufficient" => "精力不足",
            "arrived" => "已抵达",
            "npcNotFound" => "未找到此人",
            "alreadyMet" => "已经邂逅",
            "met" => "初次邂逅",
            "affinityChanged" => "好感变化",
            "gift" => "赠礼完成",
            "notMet" => "尚未邂逅",
            "giftCooldown" => "赠礼尚在冷却",
            "itemMissing" => "物品不足",
            _ => key ?? OperationFailed,
        };
        public static string AuctionLog(string key) => key switch
        {
            "refreshed" => "拍卖会已刷新",
            "playerBid" => "你参与竞拍",
            "aiOutbid" => "他人加价超过了你",
            "playerLeading" => "你暂时领先",
            "consigned" => "物品已寄售",
            "winLot" => "拍品已成交并收入背包",
            "refundBid" => "竞拍未中，押金已退回",
            "consignmentSold" => "寄售成交",
            "consignmentReturned" => "寄售流拍，物品退回",
            _ => WorldActionMessage(key),
        };
        public static string QuestLog(string key)
        {
            if (string.IsNullOrEmpty(key)) return OperationFailed;
            if (key.StartsWith("accepted:", StringComparison.Ordinal)) return $"已接受任务 {key.Substring("accepted:".Length)}";
            if (key.StartsWith("discovered:", StringComparison.Ordinal)) return $"发现任务 {key.Substring("discovered:".Length)}";
            if (key.StartsWith("delivered:", StringComparison.Ordinal)) return $"已交付 {key.Substring("delivered:".Length)}";
            if (key.StartsWith("completed:", StringComparison.Ordinal)) return $"已完成任务 {key.Substring("completed:".Length)}";
            if (key.StartsWith("exp+", StringComparison.Ordinal)) return $"{Cultivation} +{key.Substring("exp+".Length)}";
            if (key.StartsWith("gold+", StringComparison.Ordinal)) return $"{Gold} +{key.Substring("gold+".Length)}";
            if (key.StartsWith("item:", StringComparison.Ordinal)) return $"获得物品 {key.Substring("item:".Length)}";
            if (key.StartsWith("karma", StringComparison.Ordinal)) return $"{Karma} {key.Substring("karma".Length)}";
            return key;
        }
        public static string BreakthroughTarget(string name, double rate) => $"突破至 {name}（{Percent(rate * 100)}）";
        public static string BodyBreakthroughTarget(string name) => $"体修突破至 {name}";
        public static string StatBonus(string label, object value) => $"{label}+{value}";
        public static string EffectLine(string label, object value) => $"{label} {value}";
        public static string EffectDuration(int months) => $"持续 {months} 月";
        public static string SectionCount(string label, int count) => $"{label}（{count}）";
        public static string EquipmentSlotName(string slot) => slot switch { "weapon" => "武器", "helmet" => "头盔", "armor" => "护甲", "boots" => "靴履", "accessory1" => "饰品一", "accessory2" => "饰品二", _ => slot };
        public static string CategoryName(string category) => category switch { "weapon" => "武器", "armor" => "防具", "accessory" => "饰品", "pill" => "丹药", "material" => "材料", "scroll" => "卷轴", "quest" => "任务物品", _ => category ?? Unknown };
        public static string RarityName(string rarity) => rarity switch { "common" => "凡品", "uncommon" => "灵品", "rare" => "地品", "epic" => "天品", "legendary" => "仙品", _ => rarity ?? Unknown };
        public static string TechniqueTypeName(string type) => type switch { "sword" => "剑法", "blade" => "刀法", "spear" => "枪法", "fist" => "拳法", "palm" => "掌法", "finger" => "指法", _ => type ?? Unknown };
        public static string AlignmentName(string alignment) => alignment switch { "righteous" => "正道", "evil" => "魔道", "neutral" => "中立", "any" => "任意", _ => alignment ?? Unknown };
        public static string ScrollTypeName(string type) => type switch { "technique" => "功法", "divineArt" => "神通", "recipe" => "丹方", "smithingRecipe" => "炼器图谱", _ => type ?? Unknown };
        public static string StatName(string key) => key switch { "hp" or "maxHp" => Hp, "mp" or "maxMp" => Mp, "stamina" or "maxStamina" => Stamina, "mentalPower" or "maxMentalPower" => MentalPower, "atk" => Atk, "def" => Def, "speed" => Speed, "moveSpeed" => MoveSpeed, "critRate" => CritRate, "critDmgMultiplier" => CritDmg, "critResist" => CritResist, "luck" => Luck, "comprehension" => Comprehension, "charisma" => Charisma, "lifespan" => Lifespan, "physique" or "maxPhysique" => Physique, "physiqueDmgReduce" => DamageReduce, _ => key ?? Unknown };
        public static string FormatStats(Dictionary<string, double> stats) => stats == null || stats.Count == 0 ? None : string.Join("，", stats.Select(kv => StatBonus(StatName(kv.Key), kv.Value)));
        public static IEnumerable<string> DescribeEffect(StatEffect effect)
        {
            if (effect == null) yield break;
            foreach (var kv in effect.StatBonuses) yield return StatBonus(StatName(kv.Key), kv.Value);
            foreach (var kv in effect.StatMultipliers) yield return $"{StatName(kv.Key)} +{Percent(kv.Value * 100)}";
            if (Math.Abs(effect.CultivationSpeedBonus) > 0.0001) yield return $"修炼速度 +{Percent(effect.CultivationSpeedBonus * 100)}";
            if (Math.Abs(effect.BreakthroughRateBonus) > 0.0001) yield return $"突破率 +{Percent(effect.BreakthroughRateBonus * 100)}";
        }
    }
}
