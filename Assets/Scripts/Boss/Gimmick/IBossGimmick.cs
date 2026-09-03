using UnityEngine;

public interface IBossGimmick
{
    // 전투 시작 시 초기화
    void Initialize(BossManager manager);

    // 보스 체력이 깎여 기믹 페이즈를 건너뛰었을 때 발동
    void OnPhaseSkipped(int skippedCount);

    // 릴이 멈춘 직후 가계산이나 즉발 효과가 필요할 때 발동
    void OnReelStopped(SymbolData[,] grid, BattleManager battleManager);

    // 턴이 끝날 때 발동
    void OnTurnEnd();

    // 특정 좌표 오염 확인
    bool IsSlotBlocked(Vector2Int pos, SymbolType targetType);
}