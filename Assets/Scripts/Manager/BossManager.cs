using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    private BossData currentBoss;
    private float currentHP;
    private int currentPhase; // 현재 남은 체력 줄 개수
    private SymbolType currentSymbol;

    [Header("UI 연결")]
    public BossHealthBar healthBarUI;
    public BattleLogUI battleLogUI;

    public List<Vector2Int> frozenSlots = new List<Vector2Int>();

    public int remainingEarthLockTurns { get; private set; } = 0; // 남은 잠금 턴 수
    public int lockedReelIndex { get; private set; } = -1;       // 현재 잠긴 릴의 번호 (0, 1, 2)
    private bool isEarthLockJustActivated = false;

    public bool isShieldActive { get; private set; } = false;
    private float currentBreakRequirement;
    private bool isShieldJustActivated = false;
    private HashSet<int> triggeredSealPhases = new HashSet<int>();


    public SymbolType CurrentSymbol => currentSymbol;

    // 피냐타 모드 관련 변수
    public bool isDead { get; private set; } = false;
    public float accumulatedOverkill { get; private set; } = 0f;

    public void Initialize(BossData bossData)
    {
        currentBoss = bossData;
        currentPhase = currentBoss.totalPhases;
        currentHP = currentBoss.maxHPPerPhase;
        isDead = false;
        accumulatedOverkill = 0f;
        currentSymbol = currentBoss.symbolType;

        healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase);

        triggeredSealPhases.Clear();
        isShieldActive = false;
        isShieldJustActivated = false;

        remainingEarthLockTurns = 0;
        lockedReelIndex = -1;
        isEarthLockJustActivated = false;

        healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldActive);

        string initMsg = $"<color=white><b>[{currentBoss.bossName}] 출현! (HP: {currentHP} x {currentPhase}줄, 속성: {currentSymbol.ToString()})</b></color>";
        Debug.Log(initMsg);
        if (battleLogUI != null) battleLogUI.AddLog(initMsg);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            accumulatedOverkill += damage;
            string overkillMsg = $"<color=cyan>[피냐타 타격] {damage} 오버킬 누적! (총합: {accumulatedOverkill})</color>";
            Debug.Log(overkillMsg);
            if (battleLogUI != null) battleLogUI.AddLog(overkillMsg);
            return;
        }

        bool isWater = (currentSymbol == SymbolType.Water);
        bool isMetal = (currentSymbol == SymbolType.Metal);
        bool isEarth = (currentSymbol == SymbolType.Earth);

        if (isMetal && isShieldActive)
        {
            if(damage >= currentBreakRequirement)
            {
                isShieldActive = false; // 파괴 성공
                string breakMsg = $"<color=yellow>[한계 돌파] {damage} 데미지로 체력 봉인을 박살냈습니다!</color>";
                Debug.Log(breakMsg);
                if (battleLogUI != null) battleLogUI.AddLog(breakMsg);
            }
            else
            {
                string blockMsg = $"<color=grey>[봉인됨] 데미지({damage})가 부족하여 튕겨났습니다. (요구치: {currentBreakRequirement})</color>";
                Debug.Log(blockMsg);
                if (battleLogUI != null) battleLogUI.AddLog(blockMsg);
                return;
            }
        }

        string hitMsg = $"[타격] 총 {damage} 데미지 유입!";
        Debug.Log(hitMsg);
        if (battleLogUI != null) battleLogUI.AddLog(hitMsg);

        int startPhase = currentPhase;
        float remainingDamage = damage; // 깎고 남은 관통 데미지

        while(remainingDamage > 0 && currentPhase > 0)
        {
            if(remainingDamage >= currentHP)
            {
                remainingDamage -= currentHP;
                currentHP = 0;
                currentPhase--;
                
                if(currentPhase > 0)
                {
                    currentHP = currentBoss.maxHPPerPhase;
                    TriggerPhaseGimmick(currentPhase);
                }
                else
                {
                    Die(remainingDamage);
                    remainingDamage = 0;
                }
            }
            else
            {
                currentHP -= remainingDamage;
                remainingDamage = 0; // 루프 종료
                Debug.Log($"남은 HP: {currentHP} / 남은 줄: {currentPhase}");
            }
        }

        // 수 보스 기믹
        if(isWater && !isDead)
        {
            int skippedGimmickCount = 0;

            foreach(int targetPhase in currentBoss.sealPhases)
            {
                if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
                {
                    skippedGimmickCount++;
                    triggeredSealPhases.Add(targetPhase);
                }
            }

            if(skippedGimmickCount > 0)
            {
                // 중복 방지
                List<Vector2Int> availableSlots = new List<Vector2Int>();

                for(int x = 0; x < 3; ++x)
                {
                    for(int y = 0; y < 3; ++y)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (!frozenSlots.Contains(pos))
                        {
                            availableSlots.Add(pos);
                        }
                    }
                }

                for(int i = 0; i< skippedGimmickCount; ++i)
                {
                    if (availableSlots.Count == 0) break;

                    int randomIndex = Random.Range(0, availableSlots.Count);

                    Vector2Int chosenSlot = availableSlots[randomIndex];
                    frozenSlots[randomIndex] = chosenSlot;

                    availableSlots.RemoveAt(randomIndex);

                    string freezeMsg = $"<color=cyan>[빙결 발동] ({chosenSlot.x}, {chosenSlot.y}) 칸이 얼어붙었습니다!</color>";
                    Debug.Log(freezeMsg);
                    if (battleLogUI != null) battleLogUI.AddLog(freezeMsg);
                }
            }
        }

        // 토 보스 기믹
        if(isEarth && !isDead)
        {
            int skippedGimmickCount = 0;

            foreach(int targetPhase in currentBoss.sealPhases)
            {
                if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
                {
                    skippedGimmickCount++;
                    triggeredSealPhases.Add(targetPhase);
                }
            }

            if(skippedGimmickCount > 0)
            {
                remainingEarthLockTurns += skippedGimmickCount;
                isEarthLockJustActivated = true;

                if(lockedReelIndex == -1)
                {
                    lockedReelIndex = Random.Range(0, 3);
                }

                string lockMsg = "";
                if (skippedGimmickCount > 1)
                {
                    lockMsg = $"<color=#8B4513>[누적 석화] {skippedGimmickCount}개의 기믹 구간 돌파! {lockedReelIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 단단하게 굳어버립니다!</color>";
                }
                else
                {
                    lockMsg = $"<color=#8B4513>[석화 발동] 지정 페이즈 도달! {lockedReelIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 굳어버립니다!</color>";
                }

                Debug.Log(lockMsg);
                if (battleLogUI != null) battleLogUI.AddLog(lockMsg);
            }
        }

        // 금 보스 기믹
        if(isMetal && !isDead)
        {
            int skippedGimmickCount = 0;

            foreach(int targetPhase in currentBoss.sealPhases)
            {
                if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
                {
                    skippedGimmickCount++;
                    triggeredSealPhases.Add(targetPhase); // 발동 처리
                }
            }

            // 건너뛴 기믹 구간이 하나라도 있다면 발동
            if(skippedGimmickCount > 0)
            {
                isShieldActive = true;
                isShieldJustActivated = true;

                currentBreakRequirement = currentBoss.metalBaseBreakRequirement * skippedGimmickCount;

                string sealMsg = "";
                if (skippedGimmickCount > 1)
                {
                    sealMsg = $"<color=red>[누적 봉인] 단숨에 {skippedGimmickCount}개의 기믹 구간을 돌파하여, 보스가 {skippedGimmickCount}중첩 체력 봉인(요구치: {currentBreakRequirement})을 시전합니다!</color>";
                }
                else
                {
                    sealMsg = $"<color=grey>[체력 봉인] 지정 페이즈 도달! 1턴 동안 체력 봉인(요구치: {currentBreakRequirement})이 전개됩니다.</color>";
                }

                Debug.Log(sealMsg);
                if (battleLogUI != null) battleLogUI.AddLog(sealMsg);
            }
        }

        healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldActive);
    }

    public void OnTurnEnd()
    {
        if(remainingEarthLockTurns > 0)
        {
            if (isEarthLockJustActivated)
            {
                isEarthLockJustActivated = false;
            }
            else
            {
                // 1턴 차감
                remainingEarthLockTurns--;

                if (remainingEarthLockTurns <= 0)
                {
                    lockedReelIndex = -1; // 잠금 완전히 해제
                    string unlockMsg = "<color=#8B4513>[석화 해제] 굳어있던 릴의 바위가 부서지며 다시 회전할 수 있게 되었습니다!</color>";
                    Debug.Log(unlockMsg);
                    if (battleLogUI != null) battleLogUI.AddLog(unlockMsg);
                }
                else
                {
                    string remainMsg = $"<color=grey>[석화 유지] 릴 잠금이 {remainingEarthLockTurns}턴 남았습니다.</color>";
                    Debug.Log(remainMsg);
                    if (battleLogUI != null) battleLogUI.AddLog(remainMsg);
                }
            }
        }

        if (isShieldActive)
        {
            if (isShieldJustActivated) isShieldJustActivated = false;
            else
            {
                isShieldActive = false;
                string expireMsg = "<color=grey>[봉인 해제] 1턴이 지나 체력 봉인이 해제되었습니다.</color>";
                Debug.Log(expireMsg);
                if (battleLogUI != null) battleLogUI.AddLog(expireMsg);

                healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldActive);
            }
        }
    }

    private void TriggerPhaseGimmick(int phaseLeft)
    {
        Debug.Log($"[페이즈 전환] 보스의 체력 줄이 파괴되었습니다! 방해 기믹 발동! (남은 줄: {phaseLeft})");
        // TODO: 얼음, 진흙 등 기믹 발동
    }

    private void Die(float initialOverkill)
    {
        isDead = true;
        currentPhase = 0;
        currentHP = 0;
        accumulatedOverkill = initialOverkill;

        string dieMsg = "<color=red><b>[보스 처치] 보스를 성공적으로 토벌했습니다!</b></color>";
        Debug.Log(dieMsg);
        if (battleLogUI != null) battleLogUI.AddLog(dieMsg);

        // TODO: 피냐타 모드 진입 이벤트 호출 (overkillDamage 전달)
    }
}
