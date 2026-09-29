using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Effects/Plant_Corruption")]
public class ItemEffect_PlantCorruption : ItemEffect
{
    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            SlotManager targetSlot = pvpManager.currentItemTargetSlot;

            if (targetSlot.TryApplyGimmick(SlotGimmickState.Wood_Corrupted))
            {

                List<Vector2Int> available = new List<Vector2Int>();

                for (int x = 0; x < 3; x++)
                {
                    for(int y = 0; y < 3; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (!targetSlot.IsCellAffected(pos)) available.Add(pos);
                    }
                }

                if(available.Count > 0)
                {
                    System.Random rng = new System.Random(pvpManager.currentItemSeed);
                    Vector2Int targetPos = available[rng.Next(0, available.Count)];

                    targetSlot.AddActiveCell(targetPos);

                    targetSlot.gimmickSeed = pvpManager.currentItemSeed;

                    return true;
                }
            }
        }

        return false;
    }
}
