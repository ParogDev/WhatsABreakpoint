namespace WhatsABreakpoint
{
    public class KineticFusilladeStats
    {
        public float CurrentAttackSpeed { get; set; }  // attacks per second
        public float TargetAttackSpeed { get; set; }   // ideal APS for breakpoint
        public float HoveringTime { get; set; }        // time spent hovering (seconds)
        public float FiringTime { get; set; }          // time spent firing (seconds)
        public float SalvoTime { get; set; }           // total salvo time (seconds)
        public int Projectiles { get; set; }           // number of projectiles
        public float DurationModifier { get; set; }    // skill duration modifier
        public TimingStatus Status { get; set; }       // current timing status

        // Helper to get attack period in seconds
        public float GetAttackPeriod()
        {
            return CurrentAttackSpeed > 0 ? 1f / CurrentAttackSpeed : 1f;
        }

        // Helper to get difference from target
        public float GetDifference()
        {
            return CurrentAttackSpeed - TargetAttackSpeed;
        }
    }

    public enum TimingStatus
    {
        Perfect,      // Attack speed matches breakpoint
        TooFast,      // Attacking too fast, losing projectiles
        Suboptimal    // Attacking too slow, wasting potential
    }
}
