using System.Collections.Generic;

namespace ForgottenTrail
{
    public sealed class InventoryModel
    {
        private readonly Dictionary<string, int> quantities = new();
        public IReadOnlyDictionary<string, int> Quantities => quantities;
        public int Quantity(string id) => quantities.TryGetValue(id, out var value) ? value : 0;
        public bool Add(string id, int amount = 1)
        {
            if (!TrailContent.Items.ContainsKey(id) || amount <= 0) return false;
            var item = TrailContent.Items[id];
            var next = Quantity(id) + amount;
            quantities[id] = item.maxStack > 0 ? UnityEngine.Mathf.Min(next, item.maxStack) : next;
            return true;
        }
        public bool Consume(string id, int amount = 1)
        {
            if (amount <= 0 || Quantity(id) < amount) return false;
            var remaining = Quantity(id) - amount;
            if (remaining == 0) quantities.Remove(id); else quantities[id] = remaining;
            return true;
        }
        public void Restore(IEnumerable<string> ids)
        {
            quantities.Clear();
            foreach (var id in ids) Add(id);
        }
        public List<string> Snapshot()
        {
            var result = new List<string>();
            foreach (var pair in quantities) for (var i = 0; i < pair.Value; i++) result.Add(pair.Key);
            return result;
        }
    }
}
