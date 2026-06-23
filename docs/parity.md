# Issue #18 — 与网页版逐系统功能对照

审计时间：2026-06-23。范围：网页版 `D:\projects\xiuxian-game\src\game\`、`src\components\{panels,hud,layout,screens}`、`src\hooks\{core-actions,system-actions,useSaveLoad,useToast}`；Unity 版 `D:\projects\xiuxian-unity\Assets\Scripts\` 与 `LogicTests\Program.cs`。

总体结论：**45 ✅ / 11 ⚠️ / 0 ❌，共 56 项**。核心循环、战斗、经济、世界任务与多数进阶系统已有可运行 UnityEngine-free 逻辑和 LogicTests 覆盖；主要差距集中在「道侣/双修」、洪荒终局数据驱动、转世深度、进阶系统内容硬编码、以及少数 Web modal/动作入口未在 uGUI 暴露。

## 状态图例

- ✅ 完整：Unity 有对应 Data/Systems/UI，且 `LogicTests\Program.cs` 或 `Playthrough.cs` 覆盖核心逻辑。
- ⚠️ 部分：主干可用，但存在 UI 入口缺失、数据驱动不足、状态/玩法深度未完全对齐。
- ❌ 缺失：未发现 Unity 对应实现。

## `src\game` 顶层系统

| Web source | Unity counterpart | LogicTests 覆盖 | 状态 | 差距说明 |
|---|---|---|---|---|
| `src\game\alchemy.ts` | `Assets\Scripts\Systems\Economy\AlchemySystem.cs`, `Data\Types\EquipmentDefs.cs`, `UI\Panels\Batch2Panels.cs` `AlchemyPanel` | `ValidateEconomySystems` `economy.alchemy.*` | ✅ 完整 | 炼丹配方、学习解锁、消耗、品质产出均已覆盖。 |
| `src\game\ascension-loader.ts` | `Data\Loaders\DlcLoader.cs`, `Data\Types\ProgressionDefs.cs` `AscensionDef`, `Assets\StreamingAssets\dlc\*\ascensions.json` | `ValidateIds`, `PrintTotals`, `progression.ascension.blockReadyDone` | ✅ 完整 | JSON 飞升定义可加载并注册。 |
| `src\game\ascension.ts` | `Systems\Progression\AdvancedProgressionSystems.cs` `AscensionSystem` | `progression.ascension.blockReadyDone` | ⚠️ 部分 | 逻辑可检查/成功应用，但 `GameContext.cs` 与 `UI\Panels` 没有专门飞升入口/状态面板；渡劫触发结果也未做完整 UI 流程。 |
| `src\game\auction.ts` | `Systems\Economy\AuctionSystem.cs`, `UI\Panels\Batch2Panels.cs` `AuctionPanel` | `economy.auction.settle` | ✅ 完整 | 刷新、出价、结算、寄售均有 uGUI 操作。 |
| `src\game\audio.ts` | `Presentation\Audio\AudioManager.cs`, `AudioDirector.cs`, `ProceduralAudio.cs`, `UI\Hud\SoundControls.cs` | Unity headless compile；表现层无 dotnet 覆盖 | ✅ 完整 | Web 音效控制在 Unity 表现层重做，符合 Presentation 分层。 |
| `src\game\body-cultivation.ts` | `Systems\CoreCultivation\ProgressionSystems.cs` `BodyCultivationSystem`, `StatusPanel`, `ActionPanel` | `core bodyCultivation.gain`, `body breakthrough` | ✅ 完整 | 体修经验、突破、灵根体修加成均已接入。 |
| `src\game\bounty.ts` | `Systems\World\Bounty\BountySystem.cs`, `UI\Panels\Batch3Panels.cs` `BountyPanel` | `world.bounty.acceptCompleteReward` | ✅ 完整 | 悬赏生成、接取、目标推进、领取奖励可用。 |
| `src\game\breakthrough-loader.ts` | `DlcLoader.cs`, `ProgressionDefs.cs` `BreakthroughReqDef`/`TribulationDef` | `ValidateIds`, `core breakthrough.*`, `tribulation.*` | ✅ 完整 | 突破需求与渡劫定义按 DLC 加载。 |
| `src\game\chronicle.ts` | `Systems\Progression\AdvancedProgressionSystems.cs` `ChronicleSystem`, `ChroniclePanel` | `progression.chronicle.entry`, `Playthrough.cs` | ✅ 完整 | 履历记录/快照/面板可用；GameOver 自动最终化仍可在后续打磨。 |
| `src\game\data.ts` | `Data\Loaders\GameDatabase.cs`, `Assets\StreamingAssets\dlc\` | `PrintTotals`, `ValidateIds`, `ValidateDuplicateKeys` | ✅ 完整 | 数据已转为 JSON + registry。 |
| `src\game\destiny.ts` | `AdvancedProgressionSystems.cs` `DestinySystem`, `TalentPanel` | `progression.destiny.passive` | ⚠️ 部分 | 命格/天赋树逻辑和 UI 可用，但定义在 C# 静态构造中（`CoreDestinies/CoreTalents/CoreTalentNodes`），未数据驱动。 |
| `src\game\dialogue-loader.ts` | `DlcLoader.cs`, `Data\Types\WorldDefs.cs` `DialogueChainDef`, `Dialogues`/`IdleChats` registry | `world.dialogue.branchEffect` | ✅ 完整 | 对话 JSON/闲聊池加载可用。 |
| `src\game\divine-arts.ts` | `Systems\Combat\DivineArtSystem.cs`, `DivineArtsPanel` | `combat.elementCounter.damage`, `combat.divineArt.effect` | ✅ 完整 | 学习、激活、五行克制、战斗效果均有覆盖。 |
| `src\game\enlightenment.ts` | `AdvancedProgressionSystems.cs` `EnlightenmentSystem`, `EnlightenmentPanel` | `progression.enlightenment.insight` | ✅ 完整 | 顿悟、参悟点、buff 与天赋点联动可用。 |
| `src\game\equipment.ts` | `Systems\Equipment\EquipmentSystem.cs`, `EquipmentPanel` | `combat equipment.affix.aggregate`, `procedural.equip.usable` | ✅ 完整 | 装备穿脱、属性聚合、程序化装备实例可用。 |
| `src\game\event-loader.ts` | `DlcLoader.cs`, `Data\Types\EventDefs.cs`, `EventSystem.cs` | `world.event.gatedExcluded`, `world.event.applyEffect` | ✅ 完整 | JSON 事件条件、权重和效果可用。 |
| `src\game\feng-shui-mining.ts` | `Systems\Economy\MiningSystem.cs`, `MiningPanel`, `MiningSiteDef` | `economy.mining.yields` | ✅ 完整 | 风水矿脉、精力消耗、产出记录已接入。 |
| `src\game\heart-demon.ts` | `AdvancedProgressionSystems.cs` `HeartDemonSystem`, `HeartDemonPanel` | `progression.heartDemon.combat` | ✅ 完整 | 心魔值、压制、强制战斗与 UI 可用。 |
| `src\game\inventory.ts` | `Systems\Economy\InventorySystem.cs`, `InventoryPanel` | `economy.inventory.*`, save/load | ✅ 完整 | 堆叠、容量、使用、丢弃与经济系统联动可用。 |
| `src\game\item-loader.ts` | `DlcLoader.cs`, `Data\Types\ItemDef.cs` | `core:hp_pill 效果解析`, `ValidateIds` | ✅ 完整 | 物品 JSON 与效果值解析已覆盖。 |
| `src\game\karma.ts` | `AdvancedProgressionSystems.cs` `KarmaSystem`; UI 分散在状态/进阶面板 | `progression.karma.band` | ⚠️ 部分 | 正邪分段和事件记录可用；缺少 Web 那样围绕业力/正邪的完整独立呈现与事件联动深度。 |
| `src\game\learning.ts` | `AdvancedProgressionSystems.cs` `LearningSystem`, `LearningPanel` | `progression.learning.technique` | ✅ 完整 | 卷轴研读、功法/神通/丹方/炼器图谱学习可用。 |
| `src\game\map.ts` | `Systems\World\Map\MapSystem.cs`, `MapPanel`, `Presentation\Scene\*` | `world.map.travel` | ✅ 完整 | 区域解锁、旅行成本、当前区域/场景表现已接入。 |
| `src\game\npc.ts` | `Systems\World\Npc\NpcSystem.cs`, `NpcPanel`, `CompanionPanel` | `world.npc.relation`, `world.dialogue.branchEffect` | ⚠️ 部分 | 基础邂逅/好感/赠礼可用；Web `npc.ts:301-477` 的 NPC 世界模拟、动态移动/突破/死亡事件未移植；`npc.ts:478-577` 道侣/双修未移植。 |
| `src\game\primordial-endgame.ts` | `AdvancedProgressionSystems.cs` `PrimordialEndgameSystem` | `progression.endgame.complete` | ⚠️ 部分 | 可完成终局，但需求、Boss、奖励、结局文本硬编码在 C#（`AdvancedProgressionSystems.cs:127-131`），未使用 Web `PrimordialEndgameDef`/`cp-04-honghuang\endgame.ts` 数据；无 uGUI modal/入口。 |
| `src\game\pvp.ts` | `AdvancedProgressionSystems.cs` `PvpSystem`, `PvpPanel` | `progression.pvp.resolves` | ✅ 完整 | 候选、战斗、积分、记录和面板已接入。 |
| `src\game\quest-loader.ts` | `DlcLoader.cs`, `QuestChainDef`, `QuestSystem.cs` | `world.quest.completeReward` | ✅ 完整 | 任务链 JSON、发现、接受、追踪、交付、领奖可用。 |
| `src\game\ranking.ts` | `AdvancedProgressionSystems.cs` `RankingSystem`, `RankingPanel` | `progression.ranking.sort` | ✅ 完整 | 多榜单快照、NPC/玩家排序和刷新可用。 |
| `src\game\reincarnation.ts` | `AdvancedProgressionSystems.cs` `ReincarnationSystem` | `progression.reincarnation.carry` | ⚠️ 部分 | 转世可保留命格/天赋/成就并应用 legacy；缺 Web `rollPreview` 预览强化、DLC `ReincarnationEffectDef`、洪荒 legacyMultiplier 叠加、最近 10 次 snapshots 裁剪和 UI modal。 |
| `src\game\secret-realm.ts` | `Systems\World\SecretRealm\SecretRealmSystem.cs`, `SecretRealmPanel` | `world.secretRealm.completeReward` | ✅ 完整 | 秘境进入、推进、奖励、冷却、历史可用。 |
| `src\game\sect.ts` | `Systems\World\Sect\SectSystem.cs`, `SectPanel` | `world.sect.joinContribution` | ✅ 完整 | 加入门派、俸禄、任务、贡献商店展示可用；管理玩法仍显示不可用提示。 |
| `src\game\shop.ts` | `Systems\Economy\ShopSystem.cs`, `ShopPanel` | `economy.shop.buy/sell` | ✅ 完整 | 区域商品、买卖、业力/魅力价格可用。 |
| `src\game\smithing.ts` | `Systems\Economy\SmithingSystem.cs`, `SmithingPanel`, `CraftingPanel` | `economy.smithing.forge/equip` | ✅ 完整 | Web `smithing.ts` 仅为消耗矿石+灵石打造装备；未发现强化/upgrade API 或数据字段，因此 Unity forge-only 与当前 Web 对齐。 |
| `src\game\spirit-root.ts` | `PlayerFactory.cs`, `PlayerStatsSystem.GetSpiritRootDisplay`, `StatusPanel` | `player.start.spiritRoots` | ✅ 完整 | 灵根组合、资质显示、修炼倍率和体修加成可用。 |
| `src\game\technique.ts` | `Systems\Combat\TechniqueSystem.cs`, `TechniquePanel`, `LearningSystem` | `progression.learning.technique`, `procedural.technique.traits` | ✅ 完整 | 学习、升级、激活、被动与程序化词条联动可用。 |

## `src\game` 子目录

| Web source | Unity counterpart | LogicTests 覆盖 | 状态 | 差距说明 |
|---|---|---|---|---|
| `src\game\achievement\*` | `AdvancedProgressionSystems.cs` `AchievementSystem`, `AchievementPanel` | `progression.achievement.unlock` | ⚠️ 部分 | 成就可解锁并加成，但成就定义硬编码在 `AdvancedProgressionSystems.cs:142-148`，不是 DLC/JSON 数据。 |
| `src\game\bottleneck\*` | `ProgressionSystems.cs` `BottleneckSystem`, `BottleneckDef` | `bottleneck.check/activate/unlock` | ✅ 完整 | 检查、激活、解除、奖励已覆盖。 |
| `src\game\breakthrough\*` | `BreakthroughSystem` in `BreakthroughAndTribulation.cs`, `ActionPanel` | `breakthrough.success.lowRoll`, `breakthrough.fail.highRoll` | ✅ 完整 | 状态、尝试、失败累计、瓶颈联动可用。 |
| `src\game\combat\*` | `Systems\Combat\CombatSystem.cs`, `TechniqueSystem.cs`, `DivineArtSystem.cs` | `ValidateCombatSystems` | ✅ 完整 | 伤害、回合、技能、神通、装备和渡劫战斗可用。 |
| `src\game\death\*` | `Systems\Death\DeathSystem.cs`, `UI\Screens\GameOverScreen.cs` | `combat death.penalty.light` | ⚠️ 部分 | 死亡触发/惩罚/复活可用；触发器、护身物、复活方式在 C# 静态列表硬编码（`DeathSystem.cs:108-132`），未数据驱动。 |
| `src\game\dialogue\*` | `Systems\World\Dialogue\DialogueSystem.cs`, `DialogueChainDef` | `world.dialogue.branchEffect` | ✅ 完整 | 条件、分支、效果、一次性触发可用。 |
| `src\game\events\*` | `Systems\World\Events\EventSystem.cs`, `EventGenerator.cs` | `world.event.*`, `procedural.event.engine` | ✅ 完整 | 事件查询、触发、冷却、程序化事件接入可用。 |
| `src\game\player\*` | `PlayerModels.cs`, `PlayerFactory.cs`, `PlayerStatsSystem.cs` | `player.start.*`, save/load | ✅ 完整 | 创建、属性重算、追踪状态、存档往返可用。 |
| `src\game\procedural\*` | `Systems\Procedural\*Generator.cs`, `EquipmentSystem.GenerateEquip` | `ValidateProceduralSystems` | ✅ 完整 | 程序化事件/妖兽/装备/功法可复现并可进入核心系统。 |
| `src\game\quest\*` | `Systems\World\Quests\QuestSystem.cs`, `QuestPanel` | `world.quest.completeReward` | ✅ 完整 | 条件、发现、目标推进、交付、奖励可用。 |
| `src\game\registry\*` | `Data\Loaders\GameDatabase.cs`, `DlcLoader.cs` | `ValidateIds`, `ValidateDuplicateKeys` | ✅ 完整 | Map.set 覆盖语义和多包加载已移植。 |
| `src\game\tribulation\*` | `BreakthroughAndTribulation.cs` `TribulationSystem` | `tribulation.realResolver`, `tribulation.default.realResolver` | ✅ 完整 | 多波天劫与战斗 resolver 可用。 |
| `src\game\types\*` | `Data\Types\*.cs`, `Systems\*State` classes | dotnet 编译 + 各 validator | ✅ 完整 | DTO 覆盖主要 JSON；少数进阶内容（成就/命格/终局）尚缺 DTO。 |

## UI 对照：`src\components\{panels,hud,layout,screens}`

| Web source | Unity counterpart | 状态 | 差距说明 |
|---|---|---|---|
| `src\components\panels\*` | `UI\Panels\Batch1Panels.cs`, `Batch2Panels.cs`, `Batch3Panels.cs`, `PanelRegistry.cs`（27 个面板 + SavePanel） | ⚠️ 部分 | 主面板基本复刻；`CompanionPanel` 只读候选（`Batch3Panels.cs:215-241`），缺 Web `CompanionPanel.tsx` 的结为道侣/双修/解除；飞升、转世、洪荒终局仍无专门 modal/入口。 |
| `src\components\hud\*` | `UI\Hud\MainHudScreen.cs`, `SoundControls.cs`, `Presentation\Feedback\ToastContainer.cs` | ✅ 完整 | 状态栏、日志、Toast、音量/静音与事件刷新已接入。 |
| `src\components\layout\*` | `UI\Core\ScreenStack.cs`, `PanelRegistry.cs`, `MainHudScreen.cs` | ✅ 完整 | 三栏 Web 布局重做为 uGUI 导航分类 + 面板容器。 |
| `src\components\screens\*` | `UI\Screens\StartScreen.cs`, `CharacterCreationScreen.cs`, `DlcSelectScreen.cs`, `GameOverScreen.cs`, `SavePanel.cs` | ⚠️ 部分 | 开始、建角、DLC、存档、GameOver 可用；Web 的 `ReincarnationModal` / `PrimordialEndgameModal` / 更完整 SaveManager 流程未完全复刻。 |

## Hooks / actions 对照

| Web source | Unity counterpart | 状态 | 差距说明 |
|---|---|---|---|
| `src\hooks\core-actions\*`, `useCoreActions.ts` | `App\GameContext.cs` core methods: `Cultivate`, `AttemptBreakthrough`, `Rest`, travel/combat-adjacent actions | ✅ 完整 | 核心动作转为 `GameContext` 方法 + `GameEventBus` 刷新。 |
| `src\hooks\system-actions\*`, `useSystemActions.ts` | `GameContext.cs` system methods + `Panel` button callbacks | ⚠️ 部分 | 炼丹/炼器/商店/拍卖/地图/任务/NPC/门派/秘境/悬赏/进阶大多有方法；缺 Web `useNpcActions.ts` 的道侣三动作，且无飞升/转世/终局 UI action。 |
| `src\hooks\useSaveLoad.ts` | `Systems\Save\SaveSystem.cs`, `UnitySaveStorage.cs`, `SavePanel.cs` | ✅ 完整 | 多槽 JSON 存档、损坏兜底、旧字段补全由 SaveSystem 覆盖。 |
| `src\hooks\useToast.ts` | `GameContext.RequestToast`, `Presentation\Feedback\ToastSystem.cs`, `ToastContainer.cs` | ✅ 完整 | Toast 请求事件和队列显示已接入。 |

## 已核实的重点差距

1. **道侣/双修：⚠️ 大缺口。** Web 没有独立 `companion.ts`，但 `src\game\npc.ts:478-577` 实现 `getDualCultivationState`、`canFormDaoCompanion`、`formDaoCompanion`、`performDualCultivation`、`dissolveDaoCompanion`；`CompanionPanel.tsx` 和 `useNpcActions.ts` 暴露完整操作。Unity `NpcSystem.cs` 只有基础关系/赠礼，`CompanionPanel` 仅按好感展示候选，无状态和按钮。
2. **Smithing 强化：无 Web 依据。** 已搜索 `smithing.ts`/`SmithingPanel.tsx`，未发现 `upgrade`/`强化`/`enhance` API；Unity forge-only 与 Web 当前代码一致，不建议凭空加功能。
3. **洪荒终局：⚠️ 逻辑可跑但违背数据驱动。** Web 使用 `PrimordialEndgameDef` 与 `cp-04-honghuang\endgame.ts`；Unity 将境界 15、520000000 修为、`cp-04:primordial_jade`、Boss 数值、奖励和结局文本硬编码在 `AdvancedProgressionSystems.cs:127-131`。
4. **转世：⚠️ 深度不足。** Unity 保留 legacy 主干，但未实现 Web 的 `applyLegacyToPreview`、`applyDlcReincarnationEffects`、洪荒 legacy bonus 和 snapshots 只保留最近 10 次等细节。
5. **无完全缺失的顶层系统。** 本次列出的 56 项没有 ❌；但若按功能子系统计算，NPC 世界模拟与道侣/双修可视为「无完整 Unity 子系统」。
6. **数据/文案集中违规。** 主要在 `AdvancedProgressionSystems.cs`（命格/天赋/悟道/排行/成就/终局常量）、`DeathSystem.cs`（死亡触发/护身/复活内容）、`EquipmentSystem.cs`（品质数值仍硬编码）。本次已修复一个低风险文案集中问题：装备品质显示名改为引用 `ProceduralTexts`。

## 本次低风险修复

- `Assets\Scripts\Systems\Equipment\EquipmentSystem.cs`：将程序化装备品质显示名从内联中文改为 `ProceduralTexts.Quality*` 常量，减少文案 magic string。
- `LogicTests\Program.cs`：新增 `procedural.equip.qualityText` 断言，确保强制 `treasure` 品质生成名使用集中常量。

## 建议后续 Issue

1. **道侣/双修完整移植**：补 `DualCultivationState`、结为/双修/解除 API、NPC 世界模拟最小状态、CompanionPanel 操作按钮与 LogicTests。
2. **洪荒终局数据驱动**：新增 `PrimordialEndgameDef` DTO/JSON loader，复用 `cp-04-honghuang` 终局数据，移除 C# 硬编码 Boss/奖励/结局文本，并加 uGUI 入口。
3. **转世系统深度补齐**：补预览加成、DLC reincarnation effects、洪荒 legacyMultiplierBonus、snapshot 裁剪与 Reincarnation modal。
4. **进阶内容数据化**：命格/天赋/悟道 insight/排行维度/成就定义从 `AdvancedProgressionSystems.cs` 迁出到 DLC JSON。
5. **死亡系统数据化**：死亡触发器、护身物、复活方式迁出到 DTO/JSON，保留纯逻辑解释器。
6. **飞升 UI 流程**：为 `AscensionSystem` 增加 uGUI 状态/尝试入口，处理需要渡劫时的完整交互。
