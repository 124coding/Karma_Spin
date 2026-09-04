using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("테스트용 로그")]
    [SerializeField] private BattleLogUI battleLogUI;

    [Header("테스트용 데이터")]
    public BossData testBossData;

    [Header("슬롯 매니저 연결")]
    public SlotManager slotManager;

    [Header("기믹용 데이터")]
    public SymbolData badSymbol;

    [SerializeField] private BossManager bossManager;
    private SymbolType bossSymbolType;

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

    public BossManager BossManager => bossManager;

    public SymbolType BossSymbolType => bossSymbolType;

    // TODO: Test 삭제 필요
    private void Start()
    {
        SetInitialize(testBossData);
    }

    public void SetInitialize(BossData stageBossData)
    {
        bossManager.Initialize(stageBossData);
        bossSymbolType = bossManager.CurrentSymbol;

        // 턴은 배틀 매니저(스테이지 룰)가 자체적으로 결정 + 유물 효과
        int relicTurnBonus = 0;

        // TODO: 턴 증가 로직 필요
        currentTurns = baseTurnLimit + relicTurnBonus;

        Debug.Log($"전투 시작! 주어지는 총 턴 수: {currentTurns}");
    }

    public void OnReelStopped(SymbolData[,] grid)
    {
        bossManager.activeGimmick?.OnReelStopped(grid, this);

        // 최종 데미지 계산 및 정산 시작
        DamageReport finalReport = DamageCalculator.CalculateTotalDamage(grid, this);

        if (finalReport.isTaegeukJackpot)
        {
            bossManager.ClearAllGimmicks();
        }
        else if (finalReport.meltedIceCoords.Count > 0)
        {
            foreach (Vector2Int icePos in finalReport.meltedIceCoords)
            {
                bossManager.MeltIce(icePos);
            }
        }

        StartCoroutine(LogDamageRoutine(finalReport));
    }

    private IEnumerator LogDamageRoutine(DamageReport report)
    {
        battleLogUI.ClearLog();

        string startMsg = "<color=yellow>--- 데미지 정산 시작 ---</color>";
        Debug.Log(startMsg);
        battleLogUI.AddLog(startMsg);

        if (report.logs.Count == 0)
        {
            string failMsg = "<color=gray>당첨 실패... 턴이 차감됩니다.</color>";
            Debug.Log(failMsg);
            battleLogUI.AddLog(failMsg);
        }
        else
        {
            foreach(var log in report.logs)
            {
                string lineMsg = "";

                if (log.elementType == SymbolType.Bad || log.elementType == SymbolType.Taegeuk)
                {
                    string color = log.elementType == SymbolType.Taegeuk ? "yellow" : "purple";
                    string msgType = log.elementType == SymbolType.Taegeuk ? "축복" : "페널티";

                    // 기존에 작성했던 리치 텍스트를 변수에 담습니다.
                    lineMsg = $"<color={color}>[{log.elementType} {msgType} 잭팟!] <size=150%><b>x{log.multiplier}</b></size> -> 총합: {log.currentElementDamage}</color>";

                    // TODO: 흉, 태극 잭팟 연출 넣기
                }
                else
                {
                    lineMsg = $"[{log.elementType} 빙고!] <size=150%><b>x{log.multiplier}</b></size> -> [{log.elementType}] 누적: <color=orange>{log.currentElementDamage}</color>";

                    // TODO: 연출 넣기
                }

                Debug.Log(lineMsg);
                battleLogUI.AddLog(lineMsg);

                yield return new WaitForSeconds(0.5f);
            }

            string finalMsg = $"<color=red><b> 모든 속성 데미지 합산! 최종 폭발 데미지: {report.finalDamage}</b></color>";
            Debug.Log(finalMsg);
            battleLogUI.AddLog(finalMsg);

            bossManager.TakeDamage(report.finalDamage);
        }

        currentTurns--;
        string turnMsg = $"남은 턴 수: {currentTurns}";
        Debug.Log(turnMsg);
        battleLogUI.AddLog(turnMsg);

        if (currentTurns <= 0)
        {
            if (bossManager.isDead)
            {
                string clearMsg = $"<color=yellow>스테이지 클리어!</color>\n <color=yellow>최종 누적 오버킬 데미지: {bossManager.accumulatedOverkill} -> 보상으로 환산합니다.</color>";
                Debug.Log(clearMsg);
                battleLogUI.AddLog(clearMsg);
                // TODO: 오버킬 데미지를 골드나 재화로 변환하는 로직 호출
            }
            else
            {
                string failMsg = "<color=gray>턴을 모두 소모했습니다. 보스 토벌 실패 (Game Over).</color>";
                Debug.Log(failMsg);
                battleLogUI.AddLog(failMsg);
                // TODO: 게임 오버 UI 호출
            }
        }
        else
        {
            if (bossManager.isDead)
            {
                string pinataMsg = "<color=cyan>피냐타 모드 진행 중... 다음 스핀을 돌려 남은 턴을 소모하세요!</color>";
                Debug.Log(pinataMsg);
                battleLogUI.AddLog(pinataMsg);
            }
        }

        bossManager.OnTurnEnd();

        if (slotManager != null) slotManager.UnlockSpinButton();
    }
}
