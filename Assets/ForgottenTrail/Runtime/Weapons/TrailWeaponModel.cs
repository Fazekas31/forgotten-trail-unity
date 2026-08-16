using System.Collections.Generic;

namespace ForgottenTrail
{
    public readonly struct TrailWeaponAttack
    {
        public readonly bool Accepted;
        public readonly string WeaponId;
        public readonly string AttackKind;
        public readonly string AmmoItem;
        public readonly int AmmoUsed;
        public readonly float Range;
        public readonly float Damage;
        public readonly float Cooldown;
        public readonly string Reason;

        public TrailWeaponAttack(bool accepted, string weaponId, string attackKind, string ammoItem, int ammoUsed, float range, float damage, float cooldown, string reason = null)
        {
            Accepted = accepted; WeaponId = weaponId; AttackKind = attackKind; AmmoItem = ammoItem; AmmoUsed = ammoUsed;
            Range = range; Damage = damage; Cooldown = cooldown; Reason = reason;
        }
    }

    /// <summary>
    /// Small domain module mirroring the Godot weapon model. Presentation and raycast
    /// side effects stay in the player adapter; this class only owns weapon rules.
    /// </summary>
    public sealed class TrailWeaponModel
    {
        private sealed class Profile
        {
            public string id, attackKind, ammoItem;
            public int ammoPerAttack;
            public float range, damage, cooldown;
        }

        private readonly Dictionary<string, Profile> profiles = new()
        {
            ["knife"] = new Profile { id = "knife", attackKind = "melee", range = 1.75f, damage = 35f, cooldown = .28f },
            ["revolver"] = new Profile { id = "revolver", attackKind = "hitscan", range = 45f, damage = 65f, cooldown = .42f, ammoItem = "revolver_ammo", ammoPerAttack = 1 }
        };

        public string CurrentWeaponId { get; private set; }
        public bool IsAttacking { get; private set; }

        public bool Equip(string weaponId) => profiles.ContainsKey(weaponId) && !IsAttacking && (CurrentWeaponId = weaponId) != null;

        public TrailWeaponAttack TryAttack(int availableAmmo)
        {
            if (string.IsNullOrEmpty(CurrentWeaponId)) return Rejected("unarmed");
            if (IsAttacking) return Rejected("cooldown");
            var profile = profiles[CurrentWeaponId];
            if (availableAmmo < profile.ammoPerAttack) return new TrailWeaponAttack(false, profile.id, profile.attackKind, profile.ammoItem, profile.ammoPerAttack, profile.range, profile.damage, profile.cooldown, "empty_ammo");
            IsAttacking = true;
            return new TrailWeaponAttack(true, profile.id, profile.attackKind, profile.ammoItem, profile.ammoPerAttack, profile.range, profile.damage, profile.cooldown);
        }

        public void FinishAttack() => IsAttacking = false;

        private TrailWeaponAttack Rejected(string reason) => new(false, CurrentWeaponId, null, null, 0, 0, 0, 0, reason);
    }
}
