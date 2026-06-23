// ============================================================
// UiTexts.cs — centralized Chinese UI copy for the uGUI shell
// ============================================================

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
        public const string AgeUnit = "岁";
        public const string Hp = "体力";
        public const string Mp = "灵力";
        public const string Stamina = "精力";
        public const string Gold = "灵石";
        public const string Age = "年龄";
        public const string Luck = "运气";
        public const string Comprehension = "悟性";
        public const string Charisma = "魅力";
        public const string Mood = "心境";
        public const string Health = "健康";
        public const string SpiritRoot = "灵根";
        public const string Aptitude = "资质";
        public const string Cultivation = "修炼";
        public const string Inventory = "背包";
        public const string Map = "地图";
        public const string Combat = "战斗";
        public const string Sect = "门派";
        public const string Quests = "任务";
        public const string Shop = "商店";
        public const string Equipment = "装备";
        public const string Technique = "功法";
        public const string Alchemy = "炼丹";
        public const string World = "世界";

        public static string SlotTitle(int slot) => $"{Slot} {slot}";
        public static string SlotPreview(SaveSlotPreview p) => p == null || p.IsEmpty ? EmptySlot : $"{p.Name}｜第{p.GameYear}年{p.GameMonth}月｜境界 {p.RealmIndex}";
        public static string DlcCount(int enabled, int total) => $"已选 {enabled}/{total}";
        public static string PackLabel(string id) => id == "core" ? CoreDlcLocked : id;
        public static string LogNewGame(string name) => $"{name} 踏上修仙之路。";
        public static string LogLoaded(string name) => $"已读取 {name} 的存档。";
        public static string LogSaved(int slot) => $"已保存至存档槽 {slot}。";
        public static string PanelPlaceholder(string title) => $"{title}：{PlaceholderBody}";
        public static string AgeYears(int months) => $"{months / 12}{AgeUnit}";
        public static string RootSummary(PlayerSpiritRoots roots) => roots == null || roots.Roots.Count == 0 ? "无灵根" : string.Join("、", roots.Roots.ConvertAll(r => RootName(r.Type) + r.Affinity));
        public static string RootName(string type) => type switch { "metal" => "金", "wood" => "木", "water" => "水", "fire" => "火", "earth" => "土", _ => type };
        public static string GenderLabel(string gender) => gender == "female" ? Female : Male;
    }
}
