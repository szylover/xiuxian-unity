# 修仙小游戏 · Unity 重写版 🏔️

[`xiuxian-game`](https://github.com/szylover/xiuxian-game)（React + TypeScript 网页文字修仙模拟）的 **Unity 6000 + C# + uGUI 重写版**。目标是在完整移植网页版系统的基础上，强化表现力：立绘、场景、动画、特效、音效与可打包的 Windows 客户端。

## 环境要求

- Unity **6000.0.77f1**，安装 **Windows Build Support (IL2CPP/Mono)** / WindowsStandaloneSupport。
- .NET SDK **10**（用于 `LogicTests`）。
- Windows PowerShell、git；Unity 侧依赖 `com.unity.nuget.newtonsoft-json`。

## 打开与运行

1. 用 Unity Hub 打开仓库根目录：`D:\projects\xiuxian-unity`。
2. 打开场景 `Assets\Scenes\Main.unity`。
3. 运行入口是场景内的 `GameBootstrap`（`Assets\Scripts\App\GameBootstrap.cs`），它创建 `GameContext`、`SaveSystem`、`ScreenStack`，并进入开始界面。

## 验证

```powershell
# 逻辑/数据校验（无需 Unity 编辑器）
cd D:\projects\xiuxian-unity\LogicTests
dotnet run -c Release

# Unity 无头编译；本环境用 -logFile - 流式输出，不要依赖 -logFile <file>
cd D:\projects\xiuxian-unity
& "$env:ProgramFiles\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe" `
  -batchmode -nographics -quit -projectPath D:\projects\xiuxian-unity -logFile -
```

Unity 编译应看到 `Exiting batchmode successfully` 且无 `error CS`。

## Windows 构建

```powershell
cd D:\projects\xiuxian-unity
& "$env:ProgramFiles\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe" `
  -batchmode -nographics -quit -projectPath D:\projects\xiuxian-unity `
  -executeMethod BuildScript.BuildWindows -logFile -
```

构建脚本 `Assets\Editor\BuildScript.cs` 会打包 `Assets\Scenes\Main.unity`，输出 `Build\xiuxian-unity.exe`。

## 项目结构

```text
Assets\Scripts\Core\          数据源、事件总线等 UnityEngine-free 基础设施
Assets\Scripts\Data\          DTO、GameDatabase、DlcLoader（UnityEngine-free）
Assets\Scripts\Systems\       纯 C# 玩法系统（UnityEngine-free，可由 LogicTests 编译）
Assets\Scripts\App\           Unity 适配、GameContext、启动入口
Assets\Scripts\UI\            uGUI 屏幕、HUD、PanelRegistry 与面板
Assets\Scripts\Presentation\  立绘、场景、动画、VFX、音频表现层
Assets\StreamingAssets\dlc\   DLC JSON 内容包
LogicTests\                    dotnet 逻辑/数据校验工程
```

## 文档

- 架构与关键契约：[`docs\architecture.md`](docs/architecture.md)
- 美术/音频/字体资源接入：[`docs\assets-guide.md`](docs/assets-guide.md)
- 新内容、新系统、新面板开发：[`docs\dev-guide.md`](docs/dev-guide.md)
- 功能对照与已知后续：[`docs\parity.md`](docs/parity.md)
- 路线图：[`docs\roadmap.md`](docs/roadmap.md)
- AI agent 工作流： [`.github\copilot-instructions.md`](.github/copilot-instructions.md)

## 许可证

MIT
