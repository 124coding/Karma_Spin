using UnityEngine;

[CreateAssetMenu(menuName = "RelicEffect/Start_Shield")]
public class RelicEffect_StartShield : RelicEffect
{
    public float startingShieldAmount = 500f;

    public override void OnBattleStart(PlayerManager player, SlotManager slotManager)
    {
        player.currentShieldHP += startingShieldAmount;
    }
}
