using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    private BossData currentBoss;
    private float currentHP;
    private int currentPhase; // 현재 남은 체력 줄 개수
    private SymbolType currentSymbol;

    public BossData CurrentBoss => currentBoss;

    public RewardManager rewardManager;

    [Header("UI 연결")]
    public BossHealthBar healthBarUI;
    public BattleLogUI battleLogUI;

    public IGimmick activeGimmick { get; private set; }

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

        activeGimmick = CreateGimmick(currentSymbol);
        activeGimmick?.Initialize(this);

        rewardManager.InitializeBossReward(currentBoss.maxHPPerPhase, currentBoss.totalPhases);

        string initMsg = $"<color=white><b>[{currentBoss.bossName}] 출현! (HP: {currentHP} x {currentPhase}줄, 속성: {currentSymbol.ToString()})</b></color>";
        Debug.Log(initMsg);
        if (battleLogUI != null) battleLogUI.AddLog(initMsg);
    }

    private IGimmick CreateGimmick(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Earth: return new BossEarthGimmick();
            case SymbolType.Fire: return new BossFireGimmick();
            case SymbolType.Water: return new BossWaterGimmick();
            case SymbolType.Metal: return new BossMetalGimmick();
            case SymbolType.Wood: return new BossWoodGimmick();
            default: return null;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            accumulatedOverkill += damage;
            string overkillMsg = $"<color=cyan>[피냐타 타격] {damage} 오버킬 누적! (총합: {accumulatedOverkill})</color>";
            Debug.Log(overkillMsg);
            if (battleLogUI != null) battleLogUI.AddLog(overkillMsg);
            rewardManager.AddPinataGold(damage);
            return;
        }

        if (activeGimmick is BossMetalGimmick metalGimmick && metalGimmick.isShieldActive)
        {
            // 쉴드가 데미지를 막아냈다(false)면 그대로 함수 종료
            if (!metalGimmick.EvaluateShieldDefense(damage))
            {
                healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, metalGimmick.isShieldActive);
                return;
            }
        }

        if (rewardManager != null) rewardManager.AddDamage(damage);

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

        if (!isDead)
        {
            int skippedGimmickCount = 0;
            foreach (int targetPhase in currentBoss.sealPhases)
            {
                if (targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
                {
                    skippedGimmickCount++;
                    triggeredSealPhases.Add(targetPhase);
                }
            }

            if (skippedGimmickCount > 0)
            {
                // 활성화된 기믹에게 "너 스킵됐어! 발동해!" 라고 던져주기만 하면 끝
                activeGimmick?.OnPhaseSkipped(skippedGimmickCount);
            }
        }

        bool isShieldOn = (activeGimmick is BossMetalGimmick metal) && metal.isShieldActive;

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldOn);
        }
    }

    public int GetLockedReelIndex()
    {
        if (activeGimmick is BossEarthGimmick earthGimmick)
        {
            return earthGimmick.lockedReelIndex;
        }
        return -1;
    }

    public bool IsEarthLocked()
    {
        if (activeGimmick is BossEarthGimmick earthGimmick)
        {
            return earthGimmick.remainingEarthLockTurns > 0;
        }
        return false;
    }

    public void OnTurnEnd()
    {
        activeGimmick?.OnTurnEnd();
    }

    public void MeltIce(Vector2Int pos)
    {
        if (activeGimmick is BossWaterGimmick waterGimmick)
        {
            waterGimmick.MeltIce(pos);
        }
    }

    public bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        if (activeGimmick != null)
        {
            return activeGimmick.EvaluateCustomValidity(pos, targetType, s);
        }
        return null;
    }

    public void ClearAllGimmicks()
    {
        if (activeGimmick != null)
        {
            activeGimmick.ClearGimmick();
            string clearMsg = "<color=yellow>[태극의 축복] 보스의 모든 방해 기믹이 흔적도 없이 정화되었습니다!</color>";
            Debug.Log(clearMsg);
            if (battleLogUI != null) battleLogUI.AddLog(clearMsg);
        }
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
