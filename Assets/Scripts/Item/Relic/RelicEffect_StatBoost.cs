using UnityEngine;

[CreateAssetMenu(menuName = "RelicEffect/Stat_Boost")]
public class RelicEffect_StatBoost : RelicEffect
{
    [Header("증가시킬 영구 스탯량")]
    public float baseDamageBonus = 1f;
    public float lineMultiplierBonus = 1f;
    public float clusterMultiplierBonus = 1f;
    public float allMultiplierBonus = 1f;
    public float taegeukMultiplierBonus = 1f;

    public override void OnEquip(PlayerManager player, SlotManager slotManager)
    {
        player.baseDamage *= baseDamageBonus;
        player.lineMultiplier *= lineMultiplierBonus;
        player.clusterMultiplier *= clusterMultiplierBonus;
        player.allMultiplier *= allMultiplierBonus;
        player.taegeukMultiplier *= taegeukMultiplierBonus;
    }
}
