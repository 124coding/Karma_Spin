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

    public IBossGimmick activeGimmick { get; private set; }

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

        string initMsg = $"<color=white><b>[{currentBoss.bossName}] 출현! (HP: {currentHP} x {currentPhase}줄, 속성: {currentSymbol.ToString()})</b></color>";
        Debug.Log(initMsg);
        if (battleLogUI != null) battleLogUI.AddLog(initMsg);
    }

    private IBossGimmick CreateGimmick(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Earth: return new BossEarthGimmick();
            case SymbolType.Fire: return new BossFireGimmick();
            // TODO: case SymbolType.Fire: return new FireGimmick();
            // ... 나머지 속성 추가
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
            return;
        }

        bool isFire = (currentSymbol == SymbolType.Fire);
        bool isWater = (currentSymbol == SymbolType.Water);
        bool isMetal = (currentSymbol == SymbolType.Metal);
        bool isEarth = (currentSymbol == SymbolType.Earth);

        //if (isMetal && isShieldActive)
        //{
        //    if(damage >= currentBreakRequirement)
        //    {
        //        isShieldActive = false; // 파괴 성공
        //        string breakMsg = $"<color=yellow>[한계 돌파] {damage} 데미지로 체력 봉인을 박살냈습니다!</color>";
        //        Debug.Log(breakMsg);
        //        if (battleLogUI != null) battleLogUI.AddLog(breakMsg);
        //    }
        //    else
        //    {
        //        string blockMsg = $"<color=grey>[봉인됨] 데미지({damage})가 부족하여 튕겨났습니다. (요구치: {currentBreakRequirement})</color>";
        //        Debug.Log(blockMsg);
        //        if (battleLogUI != null) battleLogUI.AddLog(blockMsg);
        //        return;
        //    }
        //}

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

        //// 화 보스 기믹
        //if (isFire && !isDead)
        //{
        //    int skippedGimmickCount = 0;
        //    foreach(int targetPhase in currentBoss.sealPhases)
        //    {
        //        if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
        //        {
        //            skippedGimmickCount++;
        //            triggeredSealPhases.Add(targetPhase);
        //        }

        //        if (skippedGimmickCount > 0)
        //        {
        //            pendingFireStacks += skippedGimmickCount;
        //            string warnMsg = $"<color=red>보스가 다음 스핀에 {pendingFireStacks}개의 칸을 불태울 준비를 합니다!</color>";
        //            Debug.Log(warnMsg);
        //            if (battleLogUI != null) battleLogUI.AddLog(warnMsg);
        //        }
        //    }
        //}

        //// 수 보스 기믹
        //if(isWater && !isDead)
        //{
        //    int skippedGimmickCount = 0;

        //    foreach(int targetPhase in currentBoss.sealPhases)
        //    {
        //        if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
        //        {
        //            skippedGimmickCount++;
        //            triggeredSealPhases.Add(targetPhase);
        //        }
        //    }

        //    if(skippedGimmickCount > 0)
        //    {
        //        // 중복 방지
        //        List<Vector2Int> availableSlots = new List<Vector2Int>();

        //        for(int x = 0; x < 3; ++x)
        //        {
        //            for(int y = 0; y < 3; ++y)
        //            {
        //                Vector2Int pos = new Vector2Int(x, y);
        //                if (!frozenSlots.Contains(pos))
        //                {
        //                    availableSlots.Add(pos);
        //                }
        //            }
        //        }

        //        for(int i = 0; i< skippedGimmickCount; ++i)
        //        {
        //            if (availableSlots.Count == 0) break;

        //            int randomIndex = Random.Range(0, availableSlots.Count);

        //            Vector2Int chosenSlot = availableSlots[randomIndex];
        //            frozenSlots.Add(chosenSlot);

        //            availableSlots.RemoveAt(randomIndex);

        //            string freezeMsg = $"<color=cyan>[빙결 발동] ({chosenSlot.x}, {chosenSlot.y}) 칸이 얼어붙었습니다!</color>";
        //            Debug.Log(freezeMsg);
        //            if (battleLogUI != null) battleLogUI.AddLog(freezeMsg);
        //        }
        //    }
        //}

        //// 토 보스 기믹
        //if(isEarth && !isDead)
        //{
        //    int skippedGimmickCount = 0;

        //    foreach(int targetPhase in currentBoss.sealPhases)
        //    {
        //        if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
        //        {
        //            skippedGimmickCount++;
        //            triggeredSealPhases.Add(targetPhase);
        //        }
        //    }

        //    if(skippedGimmickCount > 0)
        //    {
        //        remainingEarthLockTurns += skippedGimmickCount;
        //        isEarthLockJustActivated = true;

        //        if(lockedReelIndex == -1)
        //        {
        //            lockedReelIndex = Random.Range(0, 3);
        //        }

        //        string lockMsg = "";
        //        if (skippedGimmickCount > 1)
        //        {
        //            lockMsg = $"<color=#8B4513>[누적 석화] {skippedGimmickCount}개의 기믹 구간 돌파! {lockedReelIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 단단하게 굳어버립니다!</color>";
        //        }
        //        else
        //        {
        //            lockMsg = $"<color=#8B4513>[석화 발동] 지정 페이즈 도달! {lockedReelIndex + 1}번째 릴이 {remainingEarthLockTurns}턴 동안 굳어버립니다!</color>";
        //        }

        //        Debug.Log(lockMsg);
        //        if (battleLogUI != null) battleLogUI.AddLog(lockMsg);
        //    }
        //}

        //// 금 보스 기믹
        //if(isMetal && !isDead)
        //{
        //    int skippedGimmickCount = 0;

        //    foreach(int targetPhase in currentBoss.sealPhases)
        //    {
        //        if(targetPhase <= startPhase && targetPhase >= currentPhase && !triggeredSealPhases.Contains(targetPhase))
        //        {
        //            skippedGimmickCount++;
        //            triggeredSealPhases.Add(targetPhase); // 발동 처리
        //        }
        //    }

        //    // 건너뛴 기믹 구간이 하나라도 있다면 발동
        //    if(skippedGimmickCount > 0)
        //    {
        //        isShieldActive = true;
        //        isShieldJustActivated = true;

        //        currentBreakRequirement = currentBoss.metalBaseBreakRequirement * skippedGimmickCount;

        //        string sealMsg = "";
        //        if (skippedGimmickCount > 1)
        //        {
        //            sealMsg = $"<color=red>[누적 봉인] 단숨에 {skippedGimmickCount}개의 기믹 구간을 돌파하여, 보스가 {skippedGimmickCount}중첩 체력 봉인(요구치: {currentBreakRequirement})을 시전합니다!</color>";
        //        }
        //        else
        //        {
        //            sealMsg = $"<color=grey>[체력 봉인] 지정 페이즈 도달! 1턴 동안 체력 봉인(요구치: {currentBreakRequirement})이 전개됩니다.</color>";
        //        }

        //        Debug.Log(sealMsg);
        //        if (battleLogUI != null) battleLogUI.AddLog(sealMsg);
        //    }
        //}

        //healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldActive);
    }

    public void OnTurnEnd()
    {
        activeGimmick?.OnTurnEnd();

        //if(remainingEarthLockTurns > 0)
        //{
        //    if (isEarthLockJustActivated)
        //    {
        //        isEarthLockJustActivated = false;
        //    }
        //    else
        //    {
        //        // 1턴 차감
        //        remainingEarthLockTurns--;

        //        if (remainingEarthLockTurns <= 0)
        //        {
        //            lockedReelIndex = -1; // 잠금 완전히 해제
        //            string unlockMsg = "<color=#8B4513>[석화 해제] 굳어있던 릴의 바위가 부서지며 다시 회전할 수 있게 되었습니다!</color>";
        //            Debug.Log(unlockMsg);
        //            if (battleLogUI != null) battleLogUI.AddLog(unlockMsg);
        //        }
        //        else
        //        {
        //            string remainMsg = $"<color=grey>[석화 유지] 릴 잠금이 {remainingEarthLockTurns}턴 남았습니다.</color>";
        //            Debug.Log(remainMsg);
        //            if (battleLogUI != null) battleLogUI.AddLog(remainMsg);
        //        }
        //    }
        //}

        //if (isShieldActive)
        //{
        //    if (isShieldJustActivated) isShieldJustActivated = false;
        //    else
        //    {
        //        isShieldActive = false;
        //        string expireMsg = "<color=grey>[봉인 해제] 1턴이 지나 체력 봉인이 해제되었습니다.</color>";
        //        Debug.Log(expireMsg);
        //        if (battleLogUI != null) battleLogUI.AddLog(expireMsg);

        //        healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase, isShieldActive);
        //    }
        //}

        //if (CurrentSymbol == SymbolType.Wood && corruptedSlots.Count > 0)
        //{
        //    SpreadWood(); // 턴이 끝날 때마다 1칸씩 스멀스멀 증식
        //}
    }

    private void TriggerPhaseGimmick(int phaseLeft)
    {
        Debug.Log($"[페이즈 전환] 보스의 체력 줄이 파괴되었습니다! 방해 기믹 발동! (남은 줄: {phaseLeft})");
        // TODO: 얼음, 진흙 등 기믹 발동
    }

    //public void SpreadWood()
    //{
    //    // 빙고칸이 꽉 찼으면 증식 중단
    //    if (corruptedSlots.Count == 0 || corruptedSlots.Count >= 9) return;

    //    List<Vector2Int> availableNeighbors = new List<Vector2Int>();

    //    // 현재 감염 칸 상하좌우 확인
    //    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

    //    foreach(var slot in corruptedSlots)
    //    {
    //        foreach(var dir in directions)
    //        {
    //            Vector2Int neighbor = slot + dir;

    //            // 슬롯머신 내부인지 확인하고 감염된 칸인지 확인
    //            if(neighbor.x >= 0 && neighbor.x < 3 && neighbor.y >= 0 && neighbor.y < 3)
    //            {
    //                if(!corruptedSlots.Contains(neighbor) && !availableNeighbors.Contains(neighbor))
    //                {
    //                    availableNeighbors.Add(neighbor);
    //                }
    //            }
    //        }
    //    }

    //    if(availableNeighbors.Count > 0)
    //    {
    //        Vector2Int target = availableNeighbors[Random.Range(0, availableNeighbors.Count)];
    //        corruptedSlots.Add(target);

    //        string spreadMsg = $"<color=green>[잠식] 기생 덩굴이 ({target.x}, {target.y}) 칸으로 뻗어나갑니다!</color>";
    //        Debug.Log(spreadMsg);
    //        if (battleLogUI != null) battleLogUI.AddLog(spreadMsg);
    //    }
    //}

    //public void PurifyWood(Vector2Int pos)
    //{
    //    if (corruptedSlots.Contains(pos))
    //    {
    //        corruptedSlots.Remove(pos);
    //        string purifyMsg = $"<color=orange>[정화] ({pos.x}, {pos.y})의 기생 덩굴이 제거되었습니다!</color>";
    //        Debug.Log(purifyMsg);
    //        if (battleLogUI != null) battleLogUI.AddLog(purifyMsg);
    //    }
    //}

    //public void MeltIce(Vector2Int pos)
    //{
    //    if (frozenSlots.Contains(pos))
    //    {
    //        frozenSlots.Remove(pos);
    //        string meltMsg = $"<color=red>[해빙] 불꽃이 ({pos.x}, {pos.y})의 얼음을 녹였습니다!</color>";
    //        Debug.Log(meltMsg);
    //        if (battleLogUI != null) battleLogUI.AddLog(meltMsg);
    //    }
    //}

    //// 수 잭팟 성공 시 방화 취소
    //public void ExtinguishAllFires()
    //{
    //    pendingFireStacks = 0;
    //    string extMsg = $"<color=blue>수 속성 잭팟! 화염이 퍼지기 전에 모두 진압했습니다!</color>";
    //    Debug.Log(extMsg);
    //    if (battleLogUI != null) battleLogUI.AddLog(extMsg);
    //}

    //public void IgnitePendingFires()
    //{
    //    if (pendingFireStacks <= 0) return;

    //    List<Vector2Int> available = new List<Vector2Int>();
    //    for(int x = 0; x < 3; ++x)
    //    {
    //        for(int y = 0; y < 3; ++y)
    //        {
    //            available.Add(new Vector2Int(x, y));
    //        }
    //    }

    //    int ignitedCount = 0;

    //    for(int i = 0; i < pendingFireStacks; i++)
    //    {
    //        if (available.Count <= 0) break;
    //        int rand = Random.Range(0, available.Count);
    //        fireSlots.Add(available[rand]);
    //        available.RemoveAt(rand);
    //        ignitedCount++;
    //    }

    //    pendingFireStacks = 0; // 스택 소모 완료
    //    string fireMsg = $"<color=red> [화염 방사] 잭팟 실패! {ignitedCount}개의 칸이 화염에 휩싸여 흉(Bad)이 되었습니다!</color>";
    //    Debug.Log(fireMsg);
    //    if (battleLogUI != null) battleLogUI.AddLog(fireMsg);
    //}


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

    public bool IsSlotBlocked(Vector2Int pos, SymbolType targetType)
    {
        if (activeGimmick != null)
        {
            return activeGimmick.IsSlotBlocked(pos, targetType);
        }
        return false;
    }
}
