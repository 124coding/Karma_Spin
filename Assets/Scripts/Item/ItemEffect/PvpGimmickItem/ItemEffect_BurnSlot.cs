using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Effects/Burn_Slot")]
public class ItemEffect_BurnSlot : ItemEffect
{
    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            // 타겟(상대방) 슬롯 매니저 찾기
            SlotManager targetSlot = pvpManager.currentItemTargetSlot;

            if (targetSlot.TryApplyGimmick(SlotGimmickState.Fire_Burned))
            {
                targetSlot.pendingActionCount += 1;

                targetSlot.gimmickSeed = pvpManager.currentItemSeed;

                // TODO: 슬롯판 전체가 붉게 깜빡이는 경고 UI 연출 켜기

                return true;
            }
        }

        return false;
    }
}
