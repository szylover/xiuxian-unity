// ============================================================
// GameBootstrap.cs — single runtime entry point for uGUI shell
// ============================================================

using UnityEngine;
using Xiuxian.Presentation.Audio;
using Xiuxian.Systems;
using Xiuxian.UI;

namespace Xiuxian.App
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private ScreenStack navigator;
        private GameContext context;
        private AudioManager audioManager;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            audioManager = EnsureAudioManager();
            context = new GameContext(new UnityStreamingAssetsDataSource(), new SaveSystem(UnitySaveStorage.Create()));
            navigator = new ScreenStack(context);
            navigator.Show<StartScreen>();
        }

        private AudioManager EnsureAudioManager()
        {
            if (AudioManager.Instance != null) return AudioManager.Instance;
            var audioObject = new GameObject("AudioManager");
            audioObject.transform.SetParent(transform, false);
            return audioObject.AddComponent<AudioManager>();
        }
    }
}
