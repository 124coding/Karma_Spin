using UnityEngine;

public abstract class RelicEffect : ScriptableObject
{
    public virtual void OnEquip(PlayerManager player, SlotManager slotManager) { }
    public virtual void OnBattleStart(PlayerManager player, SlotManager slotManager) { }

}
