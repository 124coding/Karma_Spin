using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Effects/Freeze_Slot")]
public class ItemEffect_FreezeSlot : ItemEffect
{
    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            SlotManager targetSlot = pvpManager.currentItemTargetSlot;

            if (targetSlot.TryApplyGimmick(SlotGimmickState.Water_Frozen))
            {
                System.Random rng = new System.Random(pvpManager.currentItemSeed);

                Vector2Int targetPos = new Vector2Int(rng.Next(0, 3), rng.Next(0, 3));
                targetSlot.AddActiveCell(targetPos);

                Debug.Log($"상대방의 ({targetPos.x}, {targetPos.y}) 칸을 얼렸습니다!");

                return true;
            }
        }

        return false;
    }
}
