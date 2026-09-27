using UnityEngine;

[CreateAssetMenu(menuName = "RelicEffect/Reel_Weight_Boost")]
public class RelicEffect_ReelWeight : RelicEffect
{
    [Header("릴 가중치 설정")]
    public SymbolType targetSymbol;
    public int weightBonus = 3;

    public override void OnEquip(PlayerManager player, SlotManager slotManager)
    {
        // 획득 즉시 내 슬롯 매니저에 영구 가중치를 넣고 릴을 다시 섞음
        slotManager.AddExtraWeight(targetSymbol, weightBonus);
    }
}
