# Agent 工作流（Unity / C# 版）

本仓库是 [`xiuxian-game`](https://github.com/szylover/xiuxian-game)（React + TS 网页版修仙模拟）的 **Unity（C# + uGUI）重写版**，目标是在全量移植所有系统的基础上，加强表现力（立绘 / 场景 / 动画 / 特效 / 音效）。

本项目沿用网页版的多 Agent 协作模型（适配 Unity/C#）：

| 角色 | 文件 | 职责 |
|------|------|------|
| **@PM** | `.github/agents/pm.agent.md` | 需求分析、Design Spec、任务管理、进度追踪 |
| **@Dev** | `.github/agents/dev.agent.md` | 实现 `Assets/Scripts/` 下的 C# 逻辑与 uGUI |
| **@Designer** | `.github/agents/designer.agent.md` | UI/UX、uGUI 预制体、美术/动画/特效/音效表现 |
| **@Content** | `.github/agents/content.agent.md` | 数据内容（事件/物品/妖兽/功法/NPC/任务）查漏补缺 |

## 任务管理

- **GitHub Issues 是任务的单一数据源**：https://github.com/szylover/xiuxian-unity/issues
- **任务 ID = Issue #编号**；spec 文件命名 `docs/specs/<编号>-<简称>.md`，分支命名 `feat/<编号>-<简称>`
- Issue 状态用 open/closed 管理；`docs/roadmap.md` 仅作高层路线图

### Issue 模板

```
# <编号> — <标题>

- **状态**: ⬜ 未开始 / 🔨 进行中 / ✅ 完成
- **分类**: <label>
- **前置**: <依赖的 issue 编号 或 —>
- **Spec**: docs/specs/<...>.md 或 —

## 描述
<要做什么、验收标准>
```

### Labels 分类

| Label | 含义 |
|-------|------|
| 基础设施 | 工程骨架/数据加载/存档/CI |
| 数据层 | DTO/registry/DLC 加载器 |
| 核心循环 | 玩家/属性/灵根/修炼/突破/瓶颈/渡劫 |
| 战斗系统 | 战斗/技能/神通/装备/死亡 |
| 物品与经济 | 背包/炼丹/炼器/商店/拍卖/采矿 |
| 世界与社交 | 事件/任务/对话/NPC/地图/门派/秘境/悬赏 |
| 进阶机制 | 天赋命格/悟道/正邪/心魔/排行/PvP/转世/飞升/终局/学习/成就 |
| UI体验 | uGUI 面板/HUD/导航 |
| 表现力 | 立绘/场景/动画/特效/音效 |
| DLC内容 | 内容包/扩展包数据 |

## Git 规则

- **禁止直接 push 到 main**；通过 `feat/<编号>-<简称>` 分支 + PR 合并（squash）
- 文档归文档、代码归代码，尽量分开 PR
- 提交信息附 `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`

## 架构（C# / Unity）

严格分层，**游戏逻辑与表现解耦**：

- **`Assets/Scripts/Core/`** — 基础设施（数据源抽象、事件总线、工具），UnityEngine-free
- **`Assets/Scripts/Data/`** — DTO（`Types/`）+ 注册表/加载器（`Loaders/`），**UnityEngine-free**，可同时被 Unity 与 dotnet 编译
- **`Assets/Scripts/Systems/`** — 纯 C# 游戏系统（修炼/战斗/事件…），**UnityEngine-free**，逻辑可独立于引擎运行与单测
- **`Assets/Scripts/UI/`** — uGUI MonoBehaviour 面板/控制器，只负责渲染与交互，绑定到系统服务
- **`Assets/Scripts/Presentation/`** — 立绘/动画/特效/音效表现层
- **`Assets/StreamingAssets/dlc/`** — 内容包 JSON（与网页版 `src/data/dlc/` 同构，原样复用）

## 代码规范（必须遵守，沿用网页版约定）

### 数据驱动 / 系统≠内容
- 数值与内容集中在 `Assets/StreamingAssets/dlc/` 的 JSON，逻辑从数据读取，**绝不硬编码数字**
- 系统是机制，内容通过 DLC 注册（registry）；新增内容用 DLC 包，不改系统代码

### 文案集中管理（禁止 magic string）
- 所有面向玩家的中文文本集中在文案模块（`Assets/Scripts/.../Texts` 或本地化表），**禁止**在系统/UI 代码内联硬编码中文
- 静态文本用常量，动态模板用方法

### UnityEngine-free 边界
- `Core` / `Data` / `Systems` 三层**不得 `using UnityEngine`**（除非确有必要），以保证可被 `LogicTests`（dotnet）编译与单测
- 仅 `UI` / `Presentation` 依赖 UnityEngine

### 存档/读档
- 存档序列化为 JSON 写入 `Application.persistentDataPath`；优雅处理损坏/缺失（回退新建角色）
- 旧档字段缺失要做补全（migration）

## 验证方式（CI 双校验）

1. **dotnet 逻辑校验**：`cd LogicTests && dotnet run -c Release` — 加载真实 StreamingAssets JSON，校验数据层与系统逻辑（无需 Unity 编辑器）
2. **Unity 无头编译**：
   ```
   "%ProgramFiles%\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath . -logFile -
   ```
   要求 `Assembly-CSharp.dll` 编译 0 error。

> 注：`Core`/`Data`/`Systems` 保持 UnityEngine-free 即可被 `LogicTests` 直接编译，实现引擎无关的单元验证。

## 环境

- Unity **6000.0.77f1**（Unity 6 LTS）
- dotnet **10** SDK；NuGet 仅离线包（Unity 侧用 `com.unity.nuget.newtonsoft-json`，LogicTests 用 `Newtonsoft.Json 13.0.1`）
