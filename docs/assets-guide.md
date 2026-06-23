# 资源接入指南（美术 / 音频 / 字体）

本项目的表现层默认会生成程序化占位资源，因此没有真实素材也能运行。美术、音频和字体替换遵循 **Resources 约定路径 + 数据 key**：把资源放到指定目录、命名为对应 key，运行时 provider 会自动加载；找不到则回退到程序化占位。

## 1. 立绘（portraits）

**代码依据**：`Assets\Scripts\Presentation\PresentationAssetPaths.cs`、`Presentation\Portrait\ResourcePortraitProvider.cs`、`PortraitSystem.cs`、`ProceduralPortraitProvider.cs`。

### 约定

- 基础立绘：`Assets\Resources\Portraits\{gender}_{appearance}.png`
- Resources 加载 key：`Portraits/{gender}_{appearance}`
- 境界叠层（可选）：`Assets\Resources\Portraits\Overlays\realm_{realmIndex}.png`
- Resources 加载 key：`Portraits/Overlays/realm_{realmIndex}`
- `gender` 和其他 key 会经 `PresentationAssetPaths.Normalize` 转小写、去首尾空白；缺省为 `default`。

`ResourcePortraitProvider.TryGetPortrait` 先用 `Resources.Load<Sprite>(PresentationAssetPaths.PortraitBase(...))`。如果基础 sprite 不存在，会调用 `ProceduralPortraitProvider.Create` 生成头像；如果基础 sprite 存在但 overlay 不存在，只是没有叠层，不影响立绘显示。

### 操作步骤

1. 在 Unity Project 创建目录：`Assets\Resources\Portraits\`。
2. 导入 PNG，例如 `Assets\Resources\Portraits\female_0.png`。
3. 选中图片，在 Inspector 设置 Texture Type 为 **Sprite (2D and UI)**，Apply。
4. 如需境界叠层，创建 `Assets\Resources\Portraits\Overlays\realm_3.png`，也设为 Sprite。
5. Play；创建角色时选择 female / appearance 0，会加载 `Portraits/female_0`。

### 示例

- 文件：`Assets\Resources\Portraits\male_2.png`
- 角色：`Gender = male`，`Appearance = 2`
- 加载 key：`Portraits/male_2`
- 找不到时：自动显示程序化 `procedural_portrait_male_2_<realm>`。

## 2. 场景 / 背景（scenes）

**代码依据**：`PresentationAssetPaths.SceneBackground`、`Presentation\Scene\ResourceSceneProvider.cs`、`SceneSystem.cs`、`ProceduralSceneProvider.cs`。

### 约定

- 背景图：`Assets\Resources\Scenes\{regionId}.png`
- Resources 加载 key：`Scenes/{regionId}`
- `regionId` 来自 DLC `regions.json` 的 `id` 字段；代码不会替换冒号，因此文件名可包含冒号对应 region id（如 `core:qingyun_town.png`）。如果你的平台/工具链不喜欢冒号，可先用纯程序化占位，或未来扩展 `PresentationAssetPaths.Normalize` 的映射策略并同步文档。

`ResourceSceneProvider.TryGetScene` 找不到 `Scenes/{regionId}` 时，会回退到 `ProceduralSceneProvider.Create`，根据区域五行/境界生成占位背景，并保留区域标题、描述、出口和 NPC 信息。

### 查找 region id

区域定义在：

- `Assets\StreamingAssets\dlc\core\regions.json`
- `Assets\StreamingAssets\dlc\cp-01-fanren\regions.json`
- `Assets\StreamingAssets\dlc\cp-02-goudao\regions.json`
- `Assets\StreamingAssets\dlc\cp-03-xiandao\regions.json`
- `Assets\StreamingAssets\dlc\cp-04-honghuang\regions.json`
- `Assets\StreamingAssets\dlc\cp-05-modao\regions.json`
- `Assets\StreamingAssets\dlc\exp-01-qiandao\regions.json`
- `Assets\StreamingAssets\dlc\exp-02-infinite-secret-realm\regions.json`
- `Assets\StreamingAssets\dlc\exp-03-liangjie-tiandao\regions.json`
- `Assets\StreamingAssets\dlc\exp-04-sect-war\regions.json`

PowerShell 快速列出：

```powershell
cd D:\projects\xiuxian-unity
Get-ChildItem -LiteralPath 'Assets\StreamingAssets\dlc' -Filter 'regions.json' -Recurse |
  ForEach-Object { $pkg=$_.Directory.Name; Get-Content $_.FullName -Raw | ConvertFrom-Json |
    ForEach-Object { "{0}  {1}  {2}" -f $pkg,$_.id,$_.name } }
```

常见 id 示例：`core:mortal_world`、`core:qingyun_town`、`core:beast_mountain`、`cp-01:tianan`、`cp-02:hidden_valley`、`cp-03:immortal_world`、`cp-04:primordial_world`、`cp-05:demon_valley`、`exp-01:checkin_hall`、`exp-02:infinite_gate`、`exp-03:calamity_wasteland`、`exp-04:border_fort`。

### 示例

- 文件：`Assets\Resources\Scenes\core:qingyun_town.png`
- Texture Type：Sprite (2D and UI)
- 当前区域 id：`core:qingyun_town`
- 加载 key：`Scenes/core:qingyun_town`
- 找不到时：自动生成程序化山水/雾气背景。

## 3. BGM / 音效

**代码依据**：`Assets\Scripts\Presentation\Audio\AudioManager.cs`、`AudioTunings.cs`、`ProceduralAudio.cs`、`AudioDirector.cs`。

### BGM 约定

`AudioManager.PlayBgmFor(regionId, realmTier, element)` 按顺序加载：

1. 区域 BGM：`Assets\Resources\Bgm\{regionId}.wav/.ogg/.mp3`，Resources key 为 `Bgm/{regionId}`。
2. 境界兜底 BGM：`Assets\Resources\Bgm\realm_{tier}.wav/.ogg/.mp3`，Resources key 为 `Bgm/realm_{tier}`。
3. 如果都没有，调用 `ProceduralAudio.BgmLoop` 生成 8 秒循环。

`AudioTunings.RegionBgmResourcePrefix = "Bgm/"`，`RealmBgmResourcePrefix = "Bgm/realm_"`。`AudioDirector` 会在 `PlayerCreated`、`PlayerLoaded`、`DatabaseLoaded`、`RegionChanged`、`MapChanged`、`RealmChanged` 时刷新 BGM。

### BGM 示例

- 区域循环：`Assets\Resources\Bgm\core:beast_mountain.ogg`
- 境界兜底：`Assets\Resources\Bgm\realm_4.ogg`
- Import 建议：Load Type = Streaming 或 Compressed In Memory，勾选 Loop 不是必须，`AudioManager` 会设置 `AudioSource.loop = true`。

### SFX 现状与替换方向

当前 SFX 不是从 Resources 加载，而是 `AudioManager.PlayCue` 调用 `ProceduralAudio.ClipForCue(SoundCue)` 合成 PCM。cue 定义在 `AudioTunings.SoundCue`：`CultivateTick`、`CombatHit`、`BreakthroughSuccess`、`BreakthroughFailure`、`ItemGain`、`ButtonClick`、`Death`。

如要换成真实音效，推荐低风险扩展：在 `AudioManager.PlayCue` 中先尝试 `Resources.Load<AudioClip>("Sfx/" + cue)`，找不到再走 `ProceduralAudio.ClipForCue(cue)`；素材放 `Assets\Resources\Sfx\ButtonClick.wav` 等。改代码后必须跑 Unity batchmode compile 和 `LogicTests`。

## 4. 特效（VFX）

**代码依据**：`Assets\Scripts\Presentation\Vfx\VfxLibrary.cs`、`VfxDirector.cs`、`VfxOverlay.cs`、`Presentation\Animation\PresentationTunings.cs`。

### 约定与回退

当前特效是 code-only ParticleSystem recipe，不需要外部素材。`VfxLibrary` 提供命名效果：`BreakthroughSuccess`、`RealmAdvance`、`BreakthroughFailure`、`CombatHit`、`Alchemy`、`Smithing`、`Ascension`、`Ambient`。`VfxDirector` 订阅动画 hook 和 `GameEventBus`，在突破、战斗、炼丹、炼器、飞升、地图/境界变化时播放。

### 扩展步骤

1. 在 `VfxLibrary.cs` 新增返回 `VfxEffectDescriptor` 的方法，配置颜色、持续时间、粒子数量、半径、速度、大小、形状。
2. 如需调数值，优先在 `PresentationTunings` 添加常量，避免散落 magic number。
3. 在 `VfxDirector.OnGameEvent` 或动画 hook 中调用新效果。
4. 保持 VFX 代码在 `Presentation` 层；不要让 `Core` / `Data` / `Systems` 引用 UnityEngine。

## 5. 字体 / CJK 中文显示（重要）

Issue #17 发现：默认 TextMeshPro 字体在构建中可能缺少 CJK glyph，中文会显示成方框。当前 `UIBuilder.Label`（`Assets\Scripts\UI\Core\UIBuilder.cs`）创建 `TextMeshProUGUI`，优先使用 `TMP_Settings.defaultFontAsset`；如果没有 TMP Settings，会尝试用系统字体创建运行时 TMP 字体，但这不应作为正式中文包的唯一方案。

**这是制作精美中文构建唯一必须手动完成的资源步骤。**

### 推荐步骤

1. 获取可商用/授权明确的 CJK 字体 `.ttf` / `.otf`，例如 Noto Sans SC、Source Han Sans / 思源黑体。
2. 导入 Unity：建议放 `Assets\Resources\Fonts\NotoSansSC-Regular.ttf`（也可放普通 `Assets\Fonts\`）。
3. 打开 `Window > TextMeshPro > Font Asset Creator`。
4. Source Font File 选择该 CJK 字体。
5. Atlas Population Mode 选择：
   - **Dynamic SDF**：适合完整 CJK，运行时按需补 glyph。
   - 或 Static + 自定义 Character Set：适合固定字库，但需覆盖所有中文文案。
6. 生成 TMP Font Asset，例如 `Assets\Resources\Fonts\NotoSansSC SDF.asset`。
7. 打开 `Edit > Project Settings > TextMeshPro > Settings`（或项目中的 `TMP Settings` 资源）。
8. 将 **Default Font Asset** 设置为这个 CJK TMP Font Asset；或者把它加入 Fallback Font Assets。
9. 重新运行场景/构建。`UIBuilder.Label` 创建的所有 TMP 文本会读取 `TMP_Settings.defaultFontAsset`；已有/新增面板通常无需逐个指定字体。

### 验证

- 在开始界面、角色创建、HUD 日志和各面板检查中文不再出现 □。
- Windows 构建后也要检查；Editor 能显示不代表 Player 一定包含字体资源，确保 TMP Font Asset 已保存进 `Assets` 并被 TMP Settings 引用。
