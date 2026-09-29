using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BossWoodGimmick : IGimmick
{
    private BossManager bossManager;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        if (!bossManager.currentSlotManager.TryApplyGimmick(SlotGimmickState.Wood_Corrupted)) return;

        List<Vector2Int> availableSlots = new List<Vector2Int>();

        for(int x = 0; x < 3; ++x)
        {
            for (int y = 0; y < 3; ++y)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // ½½·Ô ¸Å´ÏÀú¿¡°Ô ¿À¿°µÇÁö ¾ÊÀº Ä­À» ¹°¾îº½
                if (!bossManager.currentSlotManager.IsCellAffected(pos)) availableSlots.Add(pos);
            }
        }

        for (int i = 0; i < skippedCount; ++i)
        {
            if (availableSlots.Count == 0) break;

            int randomIndex = Random.Range(0, availableSlots.Count);

            bossManager.currentSlotManager.AddActiveCell(availableSlots[randomIndex]);
            availableSlots.RemoveAt(randomIndex);

            string spreadMsg = $"<color=green>[Àá½Ä ¹ßµ¿] ({availableSlots[randomIndex].x}, {availableSlots[randomIndex].y}) Ä­ÀÌ ±â»ý µ¢±¼¿¡ ¿À¿°µÇ¾ú½À´Ï´Ù!</color>";
            Debug.Log(spreadMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(spreadMsg);
        }
    }

    public void OnReelStopped(SymbolData[,] grid, BaseBattleManager battleManager) {}

    public void OnTurnEnd() { }

    public void ClearGimmick() { bossManager.currentSlotManager.ClearGimmick(); }
}
