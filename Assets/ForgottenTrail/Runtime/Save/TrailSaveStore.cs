using System.IO;
using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailSaveStore
    {
        private readonly string path = Path.Combine(Application.persistentDataPath, "forgotten_trail_campaign.json");
        public bool Exists => File.Exists(path);
        public bool Save(CampaignSnapshot snapshot)
        {
            try
            {
                var temp = path + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(snapshot, true));
                if (File.Exists(path)) File.Replace(temp, path, null, true);
                else File.Move(temp, path);
                return true;
            }
            catch { return false; }
        }
        public CampaignSnapshot Load()
        {
            try
            {
                if (!File.Exists(path)) return null;
                var snapshot = JsonUtility.FromJson<CampaignSnapshot>(File.ReadAllText(path));
                return snapshot != null && snapshot.schemaVersion == 1 ? snapshot : null;
            }
            catch { return null; }
        }
        public void Clear() { if (File.Exists(path)) File.Delete(path); }
    }
}
