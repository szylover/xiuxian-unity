// ============================================================
// GameBootstrap.cs — single runtime entry point for uGUI shell
// ============================================================

using UnityEngine;
using Xiuxian.Systems;
using Xiuxian.UI;

namespace Xiuxian.App
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private ScreenStack navigator;
        private GameContext context;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            context = new GameContext(new UnityStreamingAssetsDataSource(), new SaveSystem(UnitySaveStorage.Create()));
            navigator = new ScreenStack(context);
            navigator.Show<StartScreen>();
        }
    }
}
