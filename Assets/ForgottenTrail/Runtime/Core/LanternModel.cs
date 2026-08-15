namespace ForgottenTrail
{
    public sealed class LanternModel
    {
        public bool Available { get; private set; }
        public bool Lit { get; private set; }
        public bool Acquire() { if (Available) return false; Available = true; Lit = true; return true; }
        public bool Toggle() { if (!Available) return false; Lit = !Lit; return Lit; }
        public void Restore(bool available, bool lit) { Available = available; Lit = available && lit; }
    }
}
