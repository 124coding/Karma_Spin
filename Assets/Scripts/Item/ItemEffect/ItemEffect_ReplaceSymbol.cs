using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Reel_Replace_Symbol")]
public class ItemEffect_ReplaceSymbol : ItemEffect
{
    public SymbolType fromSymbol; // 바꿀 심볼 (예: Bad)
    public SymbolType toSymbol;   // 결과 심볼 (예: Taegeuk)

    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if (battleManager is PvpBattleManager pvpManager)
        {
            SlotManager targetSlot = pvpManager.CurrentSlotManager;

            targetSlot.AddTempReplacement(fromSymbol, toSymbol);

            return true;
        }
        return false;
    }
}
