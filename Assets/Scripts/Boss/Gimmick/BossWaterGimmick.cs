using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BossWaterGimmick : IGimmick
{
    private BossManager bossManager;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        if (!bossManager.currentSlotManager.TryApplyGimmick(SlotGimmickState.Water_Frozen)) return;

        List<Vector2Int> availableSlots = new List<Vector2Int>();

        for(int x = 0; x < 3; ++x)
        {
            for (int y = 0; y < 3; ++y)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (!bossManager.currentSlotManager.IsCellAffected(pos)) availableSlots.Add(pos);
            }
        }

        for(int i = 0; i < skippedCount; ++i)
        {
            if (availableSlots.Count <= 0) break;
            int randomIndex = Random.Range(0, availableSlots.Count);

            bossManager.currentSlotManager.AddActiveCell(availableSlots[randomIndex]);
            availableSlots.RemoveAt(randomIndex);

            string freezeMsg = $"<color=cyan>[빙결 발동] ({availableSlots[randomIndex].x}, {availableSlots[randomIndex].y}) 칸이 얼어붙었습니다!</color>";
            Debug.Log(freezeMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(freezeMsg);
        }
    }

    public void OnReelStopped(SymbolData[,] grid, BaseBattleManager battleManager)
    {
        // 수 속성은 패시브 빙결 기믹이므로 릴 정지 시 즉발 로직 없음
    }

    public void OnTurnEnd()
    {
    }

    public void ClearGimmick() { bossManager.currentSlotManager.ClearGimmick(); }

}
