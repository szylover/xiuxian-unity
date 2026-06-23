# 修仙小游戏 · Unity 重写版 🏔️

[`xiuxian-game`](https://github.com/szylover/xiuxian-game)（React + Vite 网页文字修仙模拟）的 **Unity（C# + uGUI）重写版**。

目标：**全量移植所有系统**，功能与网页版一致，并在此基础上**加强表现力**——立绘 / 场景 / 动画 / 特效 / 音效，不再是纯文字面板。

## 技术栈

- **引擎**：Unity 6 LTS（6000.0.77f1）
- **UI**：uGUI（Canvas / Prefab）
- **数据**：StreamingAssets JSON（与网页版 `src/data/dlc/` 同构，原样复用）+ Newtonsoft.Json
- **存档**：`Application.persistentDataPath`（JSON），零后端

## 架构

```
Assets/Scripts/
  Core/         基础设施（数据源抽象、事件总线）— UnityEngine-free
  Data/         DTO（Types/）+ 注册表/加载器（Loaders/）— UnityEngine-free
  Systems/      纯 C# 游戏系统（修炼/战斗/事件…）— UnityEngine-free
  UI/           uGUI 面板/控制器
  Presentation/ 立绘/动画/特效/音效表现层
Assets/StreamingAssets/dlc/   内容包 JSON（core + cp-01..05 + exp-01..04）
LogicTests/                   dotnet 逻辑/数据校验工程（无需 Unity 编辑器）
```

`Core`/`Data`/`Systems` 保持 **UnityEngine-free**，因此可被 `LogicTests`（dotnet）直接编译与单元验证，实现引擎无关的逻辑测试。

## 开发与验证

```powershell
# 数据/逻辑校验（无需 Unity 编辑器）
cd LogicTests; dotnet run -c Release

# Unity 无头编译校验
& "$env:ProgramFiles\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe" `
  -batchmode -nographics -quit -projectPath . -logFile -
```

## 任务

任务管理见 [GitHub Issues](https://github.com/szylover/xiuxian-unity/issues)，路线图见 [`docs/roadmap.md`](docs/roadmap.md)，协作约定见 [`.github/copilot-instructions.md`](.github/copilot-instructions.md)。

## 许可证

MIT
