using System.Collections;
using UnityEngine;

public class BossBattleManager : BaseBattleManager
{
    [Header("보스전 전용 UI")]
    public SlotManager mySlotManager;
    public Transform myPopupAnchor;

    public override SlotManager CurrentSlotManager => mySlotManager;
    public override Transform CurrentPopupAnchor => myPopupAnchor;

    [Header("테스트용 데이터")]
    public BossData testBossData;

    [Header("기믹용 데이터")]
    public SymbolData badSymbol;

    [Header("결과창 UI")]
    public BattleResultUI resultUI;

    public PlayerManager myPlayer;
    [SerializeField] private BossManager bossManager;
    [SerializeField] private RewardManager rewardManager;
    private SymbolType bossSymbolType;

    public BossManager BossManager => bossManager;

    public override SymbolType TargetSymbolType => bossSymbolType;

    public override float BaseDamage => myPlayer.baseDamage;
    public override float LineMultiplier => myPlayer.lineMultiplier;
    public override float ClusterMultiplier => myPlayer.clusterMultiplier;
    public override float AllMultiplier => myPlayer.allMultiplier;
    public override float TaegeukMultiplier => myPlayer.taegeukMultiplier;

    // TODO: Test 삭제 필요
    private void Start()
    {
        SetInitialize();
    }

    public override void SetInitialize()
    {
        bossManager.Initialize(testBossData);
        bossSymbolType = bossManager.CurrentSymbol;

        // 턴은 배틀 매니저(스테이지 룰)가 자체적으로 결정 + 유물 효과
        int relicTurnBonus = 0;

        // TODO: 턴 증가 로직 필요
        currentTurns = baseTurnLimit + relicTurnBonus;

        Debug.Log($"전투 시작! 주어지는 총 턴 수: {currentTurns}");
    }

    public override void OnReelStopped(SymbolData[,] grid)
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
                bossManager.currentSlotManager.RemoveCellEffect(icePos);
            }
        }

        StartCoroutine(LogDamageRoutine(finalReport, grid));
    }

    private IEnumerator LogDamageRoutine(DamageReport report, SymbolData[,] grid)
    {
        battleLogUI.ClearLog();

        string startMsg = "<color=yellow>--- 데미지 정산 시작 ---</color>";
        Debug.Log(startMsg);
        battleLogUI.AddLog(startMsg);

        if (report.logs.Count > 0)
        {
            yield return StartCoroutine(PlayCommonDamageAnimation(report, grid, Color.white));

            string finalMsg = $"<color=red><b> 모든 속성 데미지 합산! 최종 폭발 데미지: {report.finalDamage}</b></color>";
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

                RewardInfo rewardInfo = rewardManager.ProcessEndBattleRewards();

                if (resultUI != null)
                {
                    resultUI.ShowClear(bossManager.accumulatedOverkill, rewardInfo.artifactsObtained, rewardInfo.extraGoldFromArtifactLimit);
                }
                // TODO: 오버킬 데미지를 골드나 재화로 변환하는 로직 호출
            }
            else
            {
                string failMsg = "<color=gray>턴을 모두 소모했습니다. 보스 토벌 실패 (Game Over).</color>";
                Debug.Log(failMsg);
                battleLogUI.AddLog(failMsg);

                if (resultUI != null) resultUI.ShowGameOver();
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

        if (mySlotManager != null) mySlotManager.UnlockSpinButton();
    }
}
