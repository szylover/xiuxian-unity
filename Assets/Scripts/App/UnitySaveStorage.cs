// ============================================================
// UnitySaveStorage.cs — thin Unity adapter for persistentDataPath
// ============================================================

using System.IO;
using UnityEngine;
using Xiuxian.Systems;

namespace Xiuxian.App
{
    public static class UnitySaveStorage
    {
        public static ISaveStorage Create()
        {
            return new FileSaveStorage(Path.Combine(Application.persistentDataPath, "saves"));
        }
    }
}
