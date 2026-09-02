using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("테스트용 데이터")]
    public BossData testBossData;

    [Header("슬롯 매니저 연결")]
    public SlotManager slotManager;

    [SerializeField] private BossManager bossManager;

    [SerializeField] private int baseTurnLimit = 5; // 기본 데미지
    private int currentTurns = 0;

    [SerializeField] private float baseDamage = 100f; // 기본 데미지
    [SerializeField] private float lineMultiplier = 2f;     // 빙고 1줄당 배율
    [SerializeField] private float clusterMultiplier = 5f;  // 2x3, 3x2 클러스터 배율
    [SerializeField] private float allMultiplier = 20f; // 3x3 전체 배율
    [SerializeField] private float taegeukMultiplier = 3f;    // 태극 심볼 배율
    [SerializeField] private float badMultiplier = 0.5f;    // 흉 심볼 페널티 배율

    public int BaseTurnLimit => baseTurnLimit;
    public float BaseDamage => baseDamage;
    public float LineMultiplier => lineMultiplier;
    public float ClusterMultiplier => clusterMultiplier;
    public float AllMultiplier => allMultiplier;
    public float TaegeukMultiplier => taegeukMultiplier;
    public float BadMultiplier => badMultiplier;

<<<<<<< Updated upstream
    public void TestInitializeButton()
=======
    public BossManager BossManager => bossManager;

    public SymbolType BossSymbolType => bossSymbolType;

    // TODO: Test 삭제 필요
    private void Start()
>>>>>>> Stashed changes
    {
        SetInitialize(testBossData);
    }

    public void SetInitialize(BossData stageBossData)
    {
        bossManager.Initialize(stageBossData);

        // 턴은 배틀 매니저(스테이지 룰)가 자체적으로 결정 + 유물 효과
        int relicTurnBonus = 0;

        // TODO: 턴 증가 로직 필요
        currentTurns = baseTurnLimit + relicTurnBonus;

        Debug.Log($"전투 시작! 주어지는 총 턴 수: {currentTurns}");
    }

    public void OnReelStopped(SymbolData[,] grid)
    {
        DamageReport report = DamageCalculator.CalculateTotalDamage(grid, this);
        StartCoroutine(LogDamageRoutine(report));
    }

    private IEnumerator LogDamageRoutine(DamageReport report)
    {
        Debug.Log("<color=yellow>--- 데미지 정산 시작 ---</color>");

        if (report.logs.Count == 0)
        {
            Debug.Log("<color=gray>당첨 실패... 데미지 0</color>");
            yield break;
        }

        float currentDisplayDamage = BaseDamage;
        Debug.Log($"기본 데미지: {currentDisplayDamage}");
        yield return new WaitForSeconds(0.5f);

        // 영수증(Logs)을 하나씩 순회하며 콘솔에 출력
        foreach (var log in report.logs)
        {
            currentDisplayDamage *= log.multiplier;
            Debug.Log($"[배율 증가!] {log.multiplier}배 적용 -> 현재 데미지: <color=orange>{currentDisplayDamage}</color>");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"<color=red><b>최종 폭발 데미지: {report.finalDamage}</b></color>");

        bossManager.TakeDamage(report.finalDamage);

        currentTurns--;
        Debug.Log($"남은 턴 수: {currentTurns}");

        if(currentTurns <= 0)
        {
            if (bossManager.isDead)
            {
                // 보스를 잡은 상태로 턴을 다 썼다면 -> 스테이지 클리어!
                Debug.Log($"<color=yellow>스테이지 클리어!</color>");
                Debug.Log($"<color=yellow>최종 누적 오버킬 데미지: {bossManager.accumulatedOverkill} -> 보상으로 환산합니다.</color>");
                // TODO: 오버킬 데미지를 골드나 재화로 변환하는 로직 호출
            }
            else
            {
                // 보스를 못 잡았는데 턴을 다 썼다면 -> 게임 오버
                Debug.Log("<color=gray>턴을 모두 소모했습니다. 보스 토벌 실패 (Game Over).</color>");
                // TODO: 게임 오버 UI 호출
            }
        }
        else
        {
            // 턴이 남았다면 다음 스핀 기다리기
            if (bossManager.isDead)
            {
                Debug.Log("피냐타 모드 진행 중... 다음 스핀을 돌려 남은 턴을 소모하세요!");
            }
        }

        bossManager.OnTurnEnd();

        if (slotManager != null) slotManager.UnlockSpinButton();
    }
}
