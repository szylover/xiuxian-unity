# Roadmap — Unity 重写版

高层路线图与依赖。逐任务状态以 [GitHub Issues](https://github.com/szylover/xiuxian-unity/issues) 为准。

## 阶段 0 — 基础设施
- [x] 工程骨架 + 数据导入（129 JSON）
- [x] dotnet LogicTests 校验工程
- [x] Unity 无头编译校验链路

## 阶段 1 — 数据层移植
- [x] 物品（ItemDef）DTO + DLC 加载器 + registry
- [x] 其余实体 DTO：事件 / 妖兽 / 功法 / 神通 / 区域 / NPC / 任务 / 境界 / 突破 / 渡劫 / 装备 / 丹方 / 锻造 / 程序化模板…
- [x] 完整 DLC 加载器（按包加载全部实体）+ 数据校验

## 阶段 2 — 核心逻辑系统（C#，UnityEngine-free + LogicTests 单测）
- [x] 核心循环：玩家/属性/灵根/修炼/突破/瓶颈/渡劫
- [x] 战斗：战斗/技能/神通/装备/死亡
- [x] 经济：背包/炼丹/锻造/商店/拍卖/采矿
- [x] 世界与社交：事件/任务/对话/NPC/地图/门派/秘境/悬赏（运行时事件引擎已移植；程序化事件生成仍归 #7）
- [x] 进阶机制：天赋命格/悟道/正邪/心魔/排行/PvP/转世/飞升/洪荒终局/学习/成就/编年史（#6，UnityEngine-free Systems/Progression + LogicTests 覆盖）
- [x] 程序化生成：事件/物品/妖兽/功法（#7，UnityEngine-free Systems/Procedural + LogicTests 覆盖）
- [x] 存档/读档（#8，多槽位 JSON 存档 + UnityEngine-free SaveSystem）

## 阶段 3 — uGUI 表现层
- [x] 主场景 + 启动/角色创建/DLC 选择 + HUD + 面板容器/导航（#9，运行时 uGUI shell + Main.unity bootstrap）
- [x] 复刻 27 个面板（功能对齐网页版，#10）
- [x] 状态→UI 事件刷新机制（#11，Core typed event bus + HUD/panel refresh hook + LogicTests 覆盖）

## 阶段 4 — 表现力升级（核心增量）
- [x] 立绘/场景框架（#12，Presentation 层 provider/placeholder + HUD 接入）
- [ ] 关键动画与转场
- [ ] 粒子特效
- [ ] 音效 + BGM
- [ ] 动态反馈（飘字/镜头/震屏）

## 阶段 5 — 打磨
- [ ] 通关核心流程 + 出 PC 包冒烟
- [ ] 与网页版逐系统功能对照
- [ ] 文档与资源接入指南
