public interface IWeaponUsable
{
    public Character Character { get; set; }
    public float GetIncreasedDamagePercentage();
    public float GetIncreasedAttackSpeedPercentage();
    public float GetIncreasedReloadSpeedPercentage();
    public void Rebound(float rebound);
    public void ResetRebound();
}