using UnityEngine;

public class BossEarthGimmick : IGimmick
{
    private BossManager bossManager;
    public int remainingEarthLockTurns { get; private set; } = 0;
    private bool isEarthLockJustActivated = false;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        remainingEarthLockTurns = 0;
        isEarthLockJustActivated = false;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        remainingEarthLockTurns += skippedCount;
        isEarthLockJustActivated = true;

        if (bossManager.currentSlotManager.TryApplyGimmick(SlotGimmickState.Earth_Locked))
        {
            int targetIndex = Random.Range(0, 3);
            bossManager.currentSlotManager.activeReelIndex = targetIndex;

            string lockMsg = $"<color=#8B4513>[석화] {targetIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 굳어버립니다!</color>";
            Debug.Log(lockMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(lockMsg);
        }
    }

    public void OnReelStopped(SymbolData[,] grid, BaseBattleManager battleManager)
    {
        // 토 속성은 릴 정지 시 즉발 기믹이 없으므로 비워둠
    }

    public void OnTurnEnd()
    {
        if (remainingEarthLockTurns > 0)
        {
            if (isEarthLockJustActivated) isEarthLockJustActivated = false;
            else
            {
                remainingEarthLockTurns--;
                bossManager.currentSlotManager.ClearGimmick();
            }
        }
    }

    public void ClearGimmick() { bossManager.currentSlotManager.ClearGimmick(); }

}
