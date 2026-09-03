using UnityEngine;

public class BossEarthGimmick : IBossGimmick
{
    private BossManager bossManager;
    public int remainingEarthLockTurns { get; private set; } = 0;
    public int lockedReelIndex { get; private set; } = -1;
    private bool isEarthLockJustActivated = false;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        remainingEarthLockTurns = 0;
        lockedReelIndex = -1;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        remainingEarthLockTurns += skippedCount;
        isEarthLockJustActivated = true;

        if (lockedReelIndex == -1) lockedReelIndex = Random.Range(0, 3);

        string lockMsg = $"<color=#8B4513>[석화] {lockedReelIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 굳어버립니다!</color>";
        Debug.Log(lockMsg);
        if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(lockMsg);
    }

    public void OnReelStopped(SymbolData[,] grid, BattleManager battleManager)
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
                if (remainingEarthLockTurns <= 0) lockedReelIndex = -1; // 잠금 해제
            }
        }
    }

    public bool IsSlotBlocked(Vector2Int pos, SymbolType targetType) => false;
}
