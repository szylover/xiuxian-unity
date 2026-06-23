# 架构说明

本项目严格分层：`Core` / `Data` / `Systems` 不依赖 UnityEngine，`UI` / `Presentation` 才使用 Unity API。这样同一套玩法逻辑既能在 Unity 中运行，也能由 `LogicTests` 通过 `dotnet run -c Release` 编译和校验。

## 分层

```text
Assets\StreamingAssets\dlc\*.json
        |
        v
IDataSource -> DlcLoader -> GameDatabase
        |                    |
        v                    v
     GameContext ------> Systems operate on Player
        |
        v
 GameEventBus / GameEventType
        |
        +--> UI panels (IPanel, PanelRegistry)
        +--> PresentationController / AudioDirector / VfxDirector
```

- `Assets\Scripts\Core\IDataSource.cs`：DLC 读取抽象；`FileSystemDataSource` 用于 dotnet，`Assets\Scripts\App\UnityStreamingAssetsDataSource.cs` 用于 Unity `Application.streamingAssetsPath`。
- `Assets\Scripts\Data\Loaders\DlcLoader.cs`：按包加载 `events.json`、`items.json`、`regions.json`、`quests.json` 等实体。
- `Assets\Scripts\Data\Loaders\GameDatabase.cs`：所有 registry 的集中容器；后注册覆盖，重复 key 会计数。
- `Assets\Scripts\Systems\`：修炼、战斗、经济、世界、进阶、存档等纯 C# 系统，操作 `Player` / DTO，不引用 UnityEngine。
- `Assets\Scripts\App\GameContext.cs`：运行时应用状态与系统门面；方法如 `Cultivate`、`AttemptBreakthrough`、`TravelToRegion` 调用 Systems，更新 `CurrentPlayer` 并发布事件。
- `Assets\Scripts\UI\`：uGUI 屏幕、HUD、面板，只负责渲染与交互。
- `Assets\Scripts\Presentation\`：立绘、场景、动画、粒子、音频等表现增强。

## 数据流

1. DLC JSON 放在 `Assets\StreamingAssets\dlc\<package>\`。
2. `UnityStreamingAssetsDataSource` 或 `FileSystemDataSource` 实现 `IDataSource`。
3. `DlcLoader.LoadPackage` / `LoadAll` 把 JSON 注册进 `GameDatabase`。
4. `GameContext` 持有当前 `GameDatabase`、`Player`、`SaveSystem` 和 `GameEventBus`。
5. Systems 读取 `GameDatabase` 并修改 `Player`。
6. `GameContext.PublishPlayerChanges` / `Publish` 发布 `GameEventType`。
7. UI 面板、`PresentationController`、`AudioDirector`、`VfxDirector` 订阅事件并刷新显示、BGM、音效、特效。

## 关键契约

- **数据源**：`IDataSource`（`Assets\Scripts\Core\IDataSource.cs`）提供 `ListPackages`、`ListFiles`、`Exists`、`ReadText`。
- **事件总线**：`GameEventBus` / `GameEventType`（`Assets\Scripts\Core\Events\GameEventBus.cs`）是状态到 UI/表现层的统一通知渠道。
- **面板**：`IPanel`（`Assets\Scripts\UI\Core\PanelContracts.cs`）定义 `Build` 和 `OnGameEvent`；`PanelBase`（`Assets\Scripts\UI\Panels\PanelBase.cs`）提供重建逻辑；`PanelRegistry.GetCategories` 注册导航分类和面板实例。
- **表现控制**：`PresentationController`（`Assets\Scripts\Presentation\PresentationController.cs`）订阅事件，调用 `PortraitSystem` / `SceneSystem`，并提供 `RequestPortraitTransition`、`RequestSceneTransition` hook。
- **存档**：`ISaveStorage`、`SaveSystem`、`FileSaveStorage` 位于 `Assets\Scripts\Systems\Save\`；Unity 适配 `UnitySaveStorage.Create` 写入 `Application.persistentDataPath\saves`。
- **随机与渡劫战斗**：`IRng` / `SystemRandomRng` / `FixedRng` 在 `Systems\CoreCultivation\Rng.cs`；`RealTribulationCombatResolver` 在 `BreakthroughAndTribulation.cs`，默认接入 `CombatEngine`。
- **立绘/场景 provider**：`IPortraitProvider` / `PortraitSystem`、`ISceneProvider` / `SceneSystem` 分别在 `Presentation\Portrait\PortraitSystem.cs` 和 `Presentation\Scene\SceneSystem.cs`。
- **音频**：`AudioManager`（`Presentation\Audio\AudioManager.cs`）加载 `Resources\Bgm` 或回退 `ProceduralAudio`；`AudioDirector` 把 `GameEventType` 映射到 SFX/BGM。

## 入口

Unity 场景 `Assets\Scenes\Main.unity` 中存在 `GameBootstrap` GameObject。`Assets\Scripts\App\GameBootstrap.cs` 的 `Start` 创建 `AudioManager`、`GameContext(new UnityStreamingAssetsDataSource(), new SaveSystem(UnitySaveStorage.Create()))`、`ScreenStack`，然后显示 `StartScreen`。
