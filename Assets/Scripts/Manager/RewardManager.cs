using UnityEngine;

public class RewardInfo
{
    public int artifactsObtained = 0; // 이번 전투에서 획득한 유물 수
    public int extraGoldFromArtifactLimit = 0; // 상한선 돌파로 획득한 골드
}

public class RewardManager : MonoBehaviour
{
    [Header("보상 설정")]
    [Tooltip("피냐타 모드에서 데미지 몇 당 재화를 1 줄 지")]
    public float damageToGoldRatio = 100f;
    public int currentGold = 0;

    private float totalBossMaxHP;
    private float accumulatedDamage = 0f;

    private bool is75PercentRelicDropped = false;
    private bool is100PercentRelicDropped = false;

    public void InitializeBossReward(float maxHPPerPhase, int totalPhases)
    {
        totalBossMaxHP = maxHPPerPhase * totalPhases;
        accumulatedDamage = 0f;
        is75PercentRelicDropped = false;
        is100PercentRelicDropped= false;
    }

    public void AddDamage(float damage)
    {
        accumulatedDamage += damage;
        CheckMilestones();
    }

    private void CheckMilestones()
    {
        if(!is75PercentRelicDropped && accumulatedDamage >= totalBossMaxHP * 0.75f)
        {
            is75PercentRelicDropped = true;
            DropRelic();
        }

        if (!is100PercentRelicDropped && accumulatedDamage >= totalBossMaxHP)
        {
            is100PercentRelicDropped = true;
            DropRelic();
        }
    }

    // 오버 데미지를 줄 때마다 호출
    public void AddPinataGold(float chunkDamage)
    {
        int goldEarned = Mathf.FloorToInt(chunkDamage / damageToGoldRatio);

        if (goldEarned > 0)
        {
            currentGold += goldEarned;
            Debug.Log($"<color=yellow>피냐타 보상! {goldEarned}G 획득 (누적: {currentGold}G)</color>");
            // TODO: 화면에 동전이 우수수 떨어지는 이펙트
        }
    }

    private void DropRelic()
    {
        // TODO: 실제 무작위 유물 뽑기 및 드롭 연출
    }

    // 마일스톤 판정 후 결과를 반환하도록 수정
    public RewardInfo ProcessEndBattleRewards()
    {
        RewardInfo info = new RewardInfo();

        // (가정) 전투 중 75%, 100% 마일스톤 돌파 여부에 따라 info 값 세팅
        if (is75PercentRelicDropped) info.artifactsObtained++;
        if (is100PercentRelicDropped) info.artifactsObtained++;

        // TODO: 만약 유물 상한선을 뚫었다면 info.extraGoldFromArtifactLimit 에 골드 추가
        return info;
    }
}
