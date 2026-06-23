# 开发指南

## 新增 DLC 内容（无需改代码）

内容放在 `Assets\StreamingAssets\dlc\<package>\`。包名示例：`core`、`cp-01-fanren`、`exp-04-sect-war`。`DlcLoader.LoadPackage` 会按存在的文件加载实体：`events.json`、`items.json`、`monsters.json`、`techniques.json`、`divine-arts.json`、`regions.json`、`npcs.json`、`quests.json`、`realms.json`、`breakthrough.json`、`recipes.json`、`smithing.json`、`equips.json`、`bottlenecks.json`、`secret-realms.json`、`sects.json`、`bounties.json`、`mining-sites.json`、`auction-lots.json`、`shop.json`、`dialogues\*.json` 等。

约定：

- 使用命名空间 id，例如 `core:qingyun_town`、`cp-01:tianan`、`exp-04:border_fort`。
- 新内容优先加 JSON，不要把内容写进 Systems。
- `GameDatabase` 后注册覆盖同 key；`DuplicateKeyCounts` 可帮助发现重复。
- 新包会出现在 `GameContext.AvailablePackIds`，DLC 选择界面可启用。

示例：新增区域时在 `Assets\StreamingAssets\dlc\my-pack\regions.json` 添加带 `id`、`name`、`description` 等字段的区域；如果要配背景，再按 `docs\assets-guide.md` 放 `Assets\Resources\Scenes\my-pack:region_id.png`。

## 新增系统

1. 在 `Assets\Scripts\Systems\<Area>\` 下创建 UnityEngine-free C# 类。
2. 只依赖 `Xiuxian.Data` DTO、`Player`、纯 C# helper；随机用 `IRng`（`Systems\CoreCultivation\Rng.cs`）。
3. 如需数据，先在 `Data\Types\` 加 DTO，再在 `DlcLoader` 和 `GameDatabase` 注册。
4. 在 `GameContext` 增加应用层方法，调用系统、更新 `CurrentPlayer`，再发布合适 `GameEventType`。
5. 在 `LogicTests\Program.cs` 或 `Playthrough.cs` 添加断言/流程，覆盖核心行为。
6. 若 UI/表现层要响应，订阅 `GameEventBus`；不要从 Systems 直接调用 UI 或 Unity API。

可参考：`BreakthroughSystem.AttemptBreakthrough` + `GameContext.AttemptBreakthrough` + `GameEventType.BreakthroughChanged/RealmChanged`；`AuctionSystem` + `AuctionPanel`；`SaveSystem` + `ISaveStorage`。

## 新增面板

1. 在 `Assets\Scripts\UI\Panels\` 新增 `sealed class XxxPanel : PanelBase`。
2. 在构造函数传入 `PanelId` 和标题文案；玩家可见中文优先放 `UiTexts`。
3. 实现 `BuildContent(Transform parent)`，用 `UIBuilder.Label`、`Button`、`Card`、`ScrollList` 等构建 uGUI。
4. 覆盖 `ShouldRefreshOn(GameEventType type)`，只监听必要事件。
5. 在 `PanelContracts.cs` 增加 `PanelId`，并在 `PanelRegistry.GetCategories` 注册到对应分类。

可参考：`Batch1Panels.cs` 的 `StatusPanel` / `ActionPanel`，`Batch2Panels.cs` 的 `MapPanel` / `AuctionPanel`，`Batch3Panels.cs` 的 `BountyPanel` / `PvpPanel`，以及 `SavePanel.cs`。

## 新增表现资源

- 立绘、场景、BGM、字体按 `docs\assets-guide.md` 的 Resources 约定放置。
- 立绘/场景 provider 已有 fallback；缺资源不会阻塞运行。
- VFX 当前通过 `VfxLibrary` code recipe 扩展。
- SFX 当前为 `ProceduralAudio` 合成；如改成真实 clip fallback，代码保持在 `Presentation\Audio`。

## 分支、Issue、PR

遵循 `.github\copilot-instructions.md`：

- GitHub Issues 是任务单一数据源；任务 ID = Issue 编号。
- spec 文件命名：`docs\specs\<编号>-<简称>.md`。
- 分支命名：`feat/<编号>-<简称>`。
- 禁止直接 push 到 `main`；通过 feature branch + PR 合并（通常 squash）。
- 提交信息附：`Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`。

## 验证策略

- 只改 Markdown：通常无需构建。
- 改 `Core` / `Data` / `Systems`：必须跑 `cd LogicTests; dotnet run -c Release`。
- 改任何 `.cs` 或 Unity 资源接线：跑 Unity batchmode compile，使用 `-logFile -` 流式输出并确认无 `error CS`。
- 改构建流程：运行 `BuildScript.BuildWindows`，检查 `Build\xiuxian-unity.exe`。
