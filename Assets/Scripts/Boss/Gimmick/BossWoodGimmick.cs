using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BossWoodGimmick : IBossGimmick
{
    private BossManager bossManager;
    public List<Vector2Int> corruptedSlots { get; private set; } = new List<Vector2Int>();

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        corruptedSlots.Clear();
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        List<Vector2Int> availableSlots = new List<Vector2Int>();

        for(int x = 0; x < 3; ++x)
        {
            for (int y = 0; y < 3; ++y)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!corruptedSlots.Contains(pos))
                {
                    availableSlots.Add(pos);
                }
            }
        }

        for (int i = 0; i < skippedCount; ++i)
        {
            if (availableSlots.Count == 0) break;

            int randomIndex = Random.Range(0, availableSlots.Count);
            Vector2Int chosenSlot = availableSlots[randomIndex];
            corruptedSlots.Add(chosenSlot);
            availableSlots.RemoveAt(randomIndex);

            string spreadMsg = $"<color=green>[잠식 발동] ({chosenSlot.x}, {chosenSlot.y}) 칸이 기생 덩굴에 오염되었습니다!</color>";
            Debug.Log(spreadMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(spreadMsg);
        }
    }

    public void OnReelStopped(SymbolData[,] grid, BattleManager battleManager)
    {
        if(corruptedSlots.Count > 0)
        {
            for(int i = corruptedSlots.Count - 1; i >= 0; --i)
            {
                Vector2Int pos = corruptedSlots[i];
                SymbolData landedSymbol = grid[pos.x, pos.y];

                if (landedSymbol.type == SymbolType.Fire || landedSymbol.type == SymbolType.Earth || landedSymbol.type == SymbolType.Taegeuk)
                {
                    PurifyWood(pos);
                }
                else if (landedSymbol.type == SymbolType.Water)
                {
                    string waterMsg = $"<color=blue>[영양 공급] 덩굴이 물을 머금고 성장합니다!</color>";
                    Debug.Log(waterMsg);
                    if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(waterMsg);

                    SpreadWood();
                }
            }
        }
    }

    public void OnTurnEnd()
    {
        if(corruptedSlots.Count > 0)
        {
            SpreadWood();
        }
    }

    public void SpreadWood()
    {
        if (corruptedSlots.Count == 0 || corruptedSlots.Count >= 9) return;

        List<Vector2Int> availableNeighbors = new List<Vector2Int>();
        Vector2Int[] directions = {Vector2Int.up, Vector2Int.down, Vector2Int.right,  Vector2Int.left};

        foreach(var slot in corruptedSlots)
        {
            foreach(var dir in directions)
            {
                Vector2Int neighbor = slot + dir;
                if (neighbor.x >= 0 && neighbor.x < 3 && neighbor.y >= 0 && neighbor.y < 3)
                {
                    if (!corruptedSlots.Contains(neighbor) && !availableNeighbors.Contains(neighbor))
                    {
                        availableNeighbors.Add(neighbor);
                    }
                }
            }
        }

        if(availableNeighbors.Count > 0)
        {
            Vector2Int target = availableNeighbors[Random.Range(0, availableNeighbors.Count)];
            corruptedSlots.Add(target);

            string spreadMsg = $"<color=green>[잠식] 기생 덩굴이 ({target.x}, {target.y}) 칸으로 뻗어나갑니다!</color>";
            Debug.Log(spreadMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(spreadMsg);
        }
    }

    public void PurifyWood(Vector2Int pos)
    {
        if (corruptedSlots.Contains(pos))
        {
            corruptedSlots.Remove(pos);
            string purifyMsg = $"<color=orange>[정화] ({pos.x}, {pos.y})의 기생 덩굴이 제거되었습니다!</color>";
            Debug.Log(purifyMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(purifyMsg);
        }
    }

    public bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        if (corruptedSlots.Contains(pos))
        {
            if (targetType == SymbolType.Bad) return true;
            return false;
        }
        return null;
    }

    public void ClearGimmick()
    {
        corruptedSlots.Clear();
    }
}
