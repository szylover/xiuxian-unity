// ============================================================
// InMemorySaveStorage.cs — test save storage
// UnityEngine-free
// ============================================================

using System.Collections.Generic;
using System.Linq;

namespace Xiuxian.Systems
{
    public sealed class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<int, string> slots = new();

        public string ReadSlot(int slotIndex) => slots.TryGetValue(slotIndex, out var json) ? json : null;

        public void WriteSlot(int slotIndex, string json) => slots[slotIndex] = json;

        public IReadOnlyList<int> ListSlots() => slots.Keys.OrderBy(x => x).ToArray();

        public void DeleteSlot(int slotIndex) => slots.Remove(slotIndex);
    }
}
