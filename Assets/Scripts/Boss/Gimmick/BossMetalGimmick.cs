using UnityEngine;

public class BossMetalGimmick : IGimmick
{
    private BossManager bossManager;

    // 금(Metal) 기믹 전용 상태 변수들
    public bool isShieldActive { get; private set; } = false;
    private float currentBreakRequirement;
    private bool isShieldJustActivated = false;

    public void Initialize(BossManager bossManager)
    {
        this.bossManager = bossManager;
        isShieldActive = false;
        isShieldJustActivated = false;
        currentBreakRequirement = 0f;
    }

    public void OnPhaseSkipped(int skippedCount)
    {
        isShieldActive = true;
        isShieldJustActivated = true;

        // 보스 데이터에 있는 기본 요구치에 스킵된 개수를 곱해 요구치 산정
        currentBreakRequirement = bossManager.CurrentBoss.metalBaseBreakRequirement * skippedCount;

        string sealMsg = "";
        if(skippedCount > 1)
        {
            sealMsg = $"<color=red>[누적 봉인] 단숨에 {skippedCount}개의 기믹 구간을 돌파하여, 보스가 {skippedCount}중첩 체력 봉인(요구치: {currentBreakRequirement})을 시전합니다!</color>";
        }
        else
        {
            sealMsg = $"<color=grey>[체력 봉인] 지정 페이즈 도달! 1턴 동안 체력 봉인(요구치: {currentBreakRequirement})이 전개됩니다.</color>";
        }

        Debug.Log(sealMsg);
        if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(sealMsg);
    }

    public void OnReelStopped(SymbolData[,] grid, BaseBattleManager battleManager)
    {
        // 금 속성은 릴 정지 시 즉발 기믹이 없으므로 비워둠
    }

    // 쉴드 방어 여부를 판단
    public bool EvaluateShieldDefense(float damage)
    {
        if (!isShieldActive) return true;

        if(damage >= currentBreakRequirement)
        {
            isShieldActive = false; // 파괴 성공
            string breakMsg = $"<color=yellow>[한계 돌파] {damage} 데미지로 체력 봉인을 박살냈습니다!</color>";
            Debug.Log(breakMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(breakMsg);
            return true;
        }
        else
        {
            string blockMsg = $"<color=grey>[봉인됨] 데미지({damage})가 부족하여 튕겨났습니다. (요구치: {currentBreakRequirement})</color>";
            Debug.Log(blockMsg);
            if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(blockMsg);
            return false;
        }
    }

    public void OnTurnEnd()
    {
        if (isShieldActive)
        {
            if (isShieldJustActivated)
            {
                isShieldJustActivated = false;
            }
            else
            {
                isShieldActive = false;
                string expireMsg = "<color=grey>[봉인 해제] 1턴이 지나 체력 봉인이 해제되었습니다.</color>";
                Debug.Log(expireMsg);
                if (bossManager.battleLogUI != null) bossManager.battleLogUI.AddLog(expireMsg);
            }
        }
    }

    public void ClearGimmick() { bossManager.currentSlotManager.ClearGimmick(); }

}
