using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BossWaterGimmick : IGimmick
{
    private BossManager bossManager;

    public List<Vector2Int> frozenSlots { get; private set; } = new List<Vector2Int>();

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        frozenSlots.Clear();
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        List<Vector2Int> availableSlots = new List<Vector2Int>();

        for(int x = 0; x < 3; ++x)
        {
            for (int y = 0; y < 3; ++y)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (!frozenSlots.Contains(pos))
                {
                    availableSlots.Add(pos);
                }
            }
        }

        for(int i = 0; i < skippedCount; ++i)
        {
            if (availableSlots.Count <= 0) break;

            int randomIndex = Random.Range(0, availableSlots.Count);
            Vector2Int chosenSlot = availableSlots[randomIndex];
            frozenSlots.Add(chosenSlot);
            availableSlots.RemoveAt(randomIndex);

            string freezeMsg = $"<color=cyan>[빙결 발동] ({chosenSlot.x}, {chosenSlot.y}) 칸이 얼어붙었습니다!</color>";
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

    public void MeltIce(Vector2Int pos)
    {
        if (frozenSlots.Contains(pos))
        {
            frozenSlots.Remove(pos);
            string meltMsg = $"<color=red>[해빙] 불꽃이 ({pos.x}, {pos.y})의 얼음을 녹였습니다!</color>";
            Debug.Log(meltMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(meltMsg);
        }
    }

    public bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        if (frozenSlots.Contains(pos))
        {
            if(targetType == SymbolType.Fire)
            {
                // 타겟 속성이 불일때는 심볼이 불 혹은 태극이면 허용
                return s.type == SymbolType.Fire || s.type == SymbolType.Taegeuk;
            }
            if(targetType == SymbolType.Bad)
            {
                return true;
            }
            return false;
        }

        return null;
    }

    public void ClearGimmick()
    {
        frozenSlots.Clear();
    }
}
