// ============================================================
// FileSaveStorage.cs — path-injected save storage
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xiuxian.Systems
{
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string rootPath;

        public FileSaveStorage(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string ReadSlot(int slotIndex)
        {
            var path = GetPath(slotIndex);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void WriteSlot(int slotIndex, string json)
        {
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(GetPath(slotIndex), json);
        }

        public IReadOnlyList<int> ListSlots()
        {
            if (!Directory.Exists(rootPath)) return new int[0];
            return Enumerable.Range(0, SaveSystem.SaveSlotCount)
                .Where(i => File.Exists(GetPath(i)))
                .ToArray();
        }

        public void DeleteSlot(int slotIndex)
        {
            var path = GetPath(slotIndex);
            if (File.Exists(path)) File.Delete(path);
        }

        private string GetPath(int slotIndex) => Path.Combine(rootPath, SaveSystem.GetSlotKey(slotIndex) + ".json");
    }
}
