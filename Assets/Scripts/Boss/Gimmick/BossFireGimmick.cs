using System.Collections.Generic;
using UnityEngine;

public class BossFireGimmick : IBossGimmick
{
    private BossManager bossManager;

    // 화(Fire) 기믹 전용 상태 변수들
    public List<Vector2Int> fireSlots { get; private set; } = new List<Vector2Int>();
    public int pendingFireStacks { get; private set; } = 0;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        fireSlots.Clear();
        pendingFireStacks = 0;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        pendingFireStacks += skippedCount;
        string warnMsg = $"<color=red>[화염 폭발 예고] 보스가 다음 스핀에 {pendingFireStacks}개의 칸을 불태울 준비를 합니다!</color>";
        Debug.Log(warnMsg);
        if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(warnMsg);
    }

    public void OnReelStopped(SymbolData[,] grid, BattleManager battleManager)
    {
        if (pendingFireStacks <= 0) return;

        // 불이 붙기 전 깨끗한 상태로 '가계산'을 돌려 수 잭팟 여부 확인
        DamageReport mockReport = DamageCalculator.CalculateTotalDamage(grid, battleManager);

        if (mockReport.hasWaterJackpot)
        {
            // [카운터 성공] 수 잭팟! 방화 예고를 취소합니다.
            pendingFireStacks = 0;
            string extMsg = $"<color=blue>수 속성 잭팟! 화염이 퍼지기 전에 모두 진압했습니다!</color>";
            Debug.Log(extMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(extMsg);
        }
        else
        {
            // [카운터 실패] 잭팟 실패, 실제로 불을 지릅니다.
            IgnitePendingFires();
        }
    }

    private void IgnitePendingFires()
    {
        fireSlots.Clear();
        List<Vector2Int> available = new List<Vector2Int>();
        
        for(int x = 0; x < 3; ++x)
        {
            for(int y = 0; y < 3; ++y)
            {
                available.Add(new Vector2Int(x, y));
            }
        }

        int ignitedCount = 0;
        for(int i = 0; i < pendingFireStacks; ++i)
        {
            if (available.Count <= 0) break;
            int rand = Random.Range(0, available.Count);
            fireSlots.Add(available[rand]);
            available.RemoveAt(rand);
            ignitedCount++;
        }

        pendingFireStacks = 0;
        string fireMsg = $"<color=red>[화염 방사] 잭팟 실패! {ignitedCount}개의 칸이 화염에 휩싸여 흉(Bad)이 되었습니다!</color>";
        Debug.Log(fireMsg);
        if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(fireMsg);
    }

    public void OnTurnEnd()
    {

        // 불탄 칸 원상 복구
        if (fireSlots.Count > 0)
        {
            fireSlots.Clear();
            string clearMsg = $"<color=grey>[화염 소멸] 턴이 종료되어 빙고판의 불씨가 사그라들었습니다.</color>";
            Debug.Log(clearMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(clearMsg);
        }
    }

    public bool IsSlotBlocked(Vector2Int pos, SymbolType targetType)
    {
        if (fireSlots.Contains(pos))
        {
            return targetType != SymbolType.Bad;
        }
        return false;
    }
}
