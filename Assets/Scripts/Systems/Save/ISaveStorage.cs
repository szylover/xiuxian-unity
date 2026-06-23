// ============================================================
// ISaveStorage.cs — slot-based save storage abstraction
// UnityEngine-free
// ============================================================

using System.Collections.Generic;

namespace Xiuxian.Systems
{
    public interface ISaveStorage
    {
        string ReadSlot(int slotIndex);
        void WriteSlot(int slotIndex, string json);
        IReadOnlyList<int> ListSlots();
        void DeleteSlot(int slotIndex);
    }
}
