using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Boss/BossData")]
public class BossData : ScriptableObject
{
    [Header("기본 정보")]
    public string bossName; // 보스 이름
    public float maxHPPerPhase; // 페이즈 1개당 체력
    public int totalPhases = 3; // 총 체력 줄 개수

    // TODO: 추후 페이즈가 넘어갈 때마다 발동할 기믹 데이터 리스트를 여기에 추가할 수 있습니다.
}
