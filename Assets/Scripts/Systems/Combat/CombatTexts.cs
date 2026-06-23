// ============================================================
// CombatTexts.cs — centralized combat/death/equipment text
// UnityEngine-free
// ============================================================

namespace Xiuxian.Systems
{
    public static class CombatTexts
    {
        public const string PlayerName = "你";
        public const string PlayerFirst = "⚡ 你抢得先机！";
        public const string Timeout = "⏳ 战斗久拖不决，双方暂且罢手。";
        public static string Encounter(string name, int hp) => $"⚔️ 遭遇 {name}（体力 {hp}）";
        public static string MonsterFirst(string name) => $"⚡ {name} 先发制人！";
        public static string RoundHeader(int round) => $"—— 第 {round} 回合 ——";
        public static string NormalHit(string a, string d, int dmg) => $"{a} 攻击 {d}，造成 {dmg} 伤害。";
        public static string Crit(string a, string d, int dmg) => $"💥 {a} 暴击 {d}，造成 {dmg} 伤害！";
        public static string Dodge(string d) => $"💨 {d} 闪避了攻击。";
        public static string Victory(string name) => $"✅ 击败 {name}！";
        public static string Defeat(string name) => $"💀 被 {name} 击败。";
        public static string Rewards(int exp, int gold) => $"获得修为 {exp}、灵石 {gold}。";
        public static string SkillUse(string tech, string skill, int hits, int dmg, int cost) => hits > 1
            ? $"✨ {tech}·{skill} 连击 {hits} 段，造成 {dmg} 伤害，消耗灵力 {cost}。"
            : $"✨ {tech}·{skill} 造成 {dmg} 伤害，消耗灵力 {cost}。";
        public static string ArtUse(string art, int hits, int dmg, int cost) => hits > 1
            ? $"🌟 神通 {art} 连击 {hits} 段，造成 {dmg} 伤害，消耗灵力 {cost}。"
            : $"🌟 神通 {art} 造成 {dmg} 伤害，消耗灵力 {cost}。";
        public static string SegmentHit(int hit, int dmg) => $"  第 {hit} 段命中，伤害 {dmg}。";
        public static string SegmentCrit(int hit, int dmg) => $"  第 {hit} 段暴击，伤害 {dmg}！";
        public static string SegmentDodge(int hit) => $"  第 {hit} 段被闪避。";
        public static string ElementCounter(double mult) => $"五行克制生效，伤害 ×{mult:0.##}。";
        public static string Heal(string source, int amount) => $"{source} 回复体力 {amount}。";
        public static string Shield(string source, int amount, int duration) => $"{source} 获得护盾 {amount}，持续 {duration} 回合。";
        public static string ShieldBlock(string source, int amount) => $"{source} 抵消 {amount} 伤害。";
        public static string ShieldExpire(string source) => $"{source} 护盾消散。";
        public static string DebuffDef(string name, int amount, int duration) => $"{name} 防御降低 {amount}，持续 {duration} 回合。";
        public static string DebuffAtk(string name, int amount, int duration) => $"{name} 攻击降低 {amount}，持续 {duration} 回合。";
        public static string DotApply(string name, int amount, int duration) => $"{name} 受到持续伤害 {amount}，持续 {duration} 回合。";
        public static string DotTick(string name, int amount) => $"{name} 受到持续伤害 {amount}。";
        public static string BodyExp(int amount) => $"体修修为 +{amount}。";
        public static string WaveWin(string name, int hp) => $"⚡ 渡过 {name}，剩余体力 {hp}。";
        public static string WaveLose(string name) => $"⚡ 未能渡过 {name}。";
        public static string WaveEffect(string description) => $"⚡ {description}";
    }

    public static class EquipmentTexts
    {
        public static string Unknown(string id) => $"未知装备：{id}";
        public static string RealmInsufficient(string name) => $"境界不足，无法装备 {name}。";
        public static string NoItem(string name) => $"背包中没有 {name}。";
        public static string Equipped(string name) => $"已装备 {name}。";
        public static string Unequipped(string name) => $"已卸下 {name}。";
        public const string SlotEmpty = "该槽位没有装备。";
    }

    public static class DeathTexts
    {
        public static string LifeSaverBlock(string name) => $"💎 {name}碎裂！抵消了致命伤害！";
        public static string Death(string name, string desc) => $"💀 {name}！{desc}";
        public static string Revival(string name, string desc) => $"🔮 {name}！{desc}";
        public static string ConsumeRevival(string name) => $"📦 消耗了 {name}";
        public static string ExpLoss(int value) => $"修为损失 {value}";
        public static string GoldLoss(int value) => $"灵石损失 {value}";
        public static string ItemLoss(int value) => $"遗失物品 {value} 件";
        public static string HealthLoss(int value) => $"健康降低 {value}";
        public static string MoodLoss(int value) => $"心情降低 {value}";
        public static string RealmDrop(int value) => $"境界跌落 {value} 层";
    }
}
