using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Reel_Add_Weight")]
public class ItemEffect_AddWeight : ItemEffect
{
    public SymbolType targetSymbol;
    public int addAmount = 5;

    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            SlotManager targetSlot = pvpManager.CurrentSlotManager;

            targetSlot.AddTempExtraWeights(targetSymbol, addAmount);

            targetSlot.SettingReels(pvpManager.currentItemSeed);

            return true;
        }

        return false;
    }
}
