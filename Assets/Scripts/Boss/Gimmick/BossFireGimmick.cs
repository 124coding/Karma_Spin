using System.Collections.Generic;
using UnityEngine;

public class BossFireGimmick : IGimmick
{
    private BossManager bossManager;

    // 화(Fire) 기믹 전용 상태 변수들
    public int pendingFireStacks { get; private set; } = 0;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        pendingFireStacks = 0;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        pendingFireStacks += skippedCount;
        string warnMsg = $"<color=red>[화염 폭발 예고] 보스가 다음 스핀에 {pendingFireStacks}개의 칸을 불태울 준비를 합니다!</color>";
        Debug.Log(warnMsg);
        if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(warnMsg);
    }

    public void OnReelStopped(SymbolData[,] grid, BaseBattleManager battleManager)
    {
        if (pendingFireStacks <= 0) return;

        // 불이 붙기 전 깨끗한 상태로 '가계산'을 돌려 수 잭팟 여부 확인
        DamageReport mockReport = DamageCalculator.CalculateTotalDamage(grid, battleManager);

        if (mockReport.hasWaterJackpot) pendingFireStacks = 0;
        else IgnitePendingFires();
    }

    private void IgnitePendingFires()
    {
        if (!bossManager.currentSlotManager.TryApplyGimmick(SlotGimmickState.Fire_Burned)) return;

        // 통합 예약 카운터 사용
        bossManager.currentSlotManager.pendingActionCount += pendingFireStacks;
        bossManager.currentSlotManager.ProcessPendingFires();

        pendingFireStacks = 0;
    }

    public void OnTurnEnd()
    {
    }

    public void ClearGimmick() { bossManager.currentSlotManager.ClearGimmick(); }

}
