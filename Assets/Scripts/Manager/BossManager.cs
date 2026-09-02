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

        string hitMsg = $"[타격] 총 {damage} 데미지 유입!";
        Debug.Log(hitMsg);
        if (battleLogUI != null) battleLogUI.AddLog(hitMsg);

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

        healthBarUI.UpdateHealthUI(currentHP, currentBoss.maxHPPerPhase, currentPhase);
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
