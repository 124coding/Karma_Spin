using UnityEngine;
using System.Collections;

public abstract class BaseBattleManager : MonoBehaviour
{
    [Header("테스트용 로그")]
    [SerializeField] public BattleLogUI battleLogUI;

    [Header("팝업 연출 UI")]
    public MultiplierPopup popupPrefab; // 앞서 만든 DamagePopup.cs가 붙은 프리팹

    [Header("공통 전투 룰")]
    public int baseTurnLimit = 5;       // 인스펙터에서 기본 턴 설정
    protected int currentTurns = 0;     // 자식 클래스에서 접근할 수 있도록 protected 사용

    // 외부(UI 등)에서 현재 남은 턴을 읽어갈 수 있도록 제공
    public int CurrentTurns => currentTurns;

    public abstract SlotManager CurrentSlotManager { get; }
    public abstract Transform CurrentPopupAnchor { get; }

    public abstract float BaseDamage { get; } // 기본 데미지
    public abstract float LineMultiplier { get; }   // 빙고 1줄당 배율
    public abstract float ClusterMultiplier { get; }  // 2x3, 3x2 클러스터 배율
    public abstract float AllMultiplier { get; } // 3x3 전체 배율
    public abstract float TaegeukMultiplier { get; }    // 태극 심볼 배율
    public abstract float BadMultiplier { get; }    // 흉 심볼 페널티 배율

    public virtual int GetLockedReelIndex()
    {
        return -1;
    }

    public abstract SymbolType TargetSymbolType { get; }

    public virtual bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        return null;
    }

    public abstract void SetInitialize();

    public abstract void OnReelStopped(SymbolData[,] grid);

    protected void ShowDamagePopup(string text, Color color)
    {
        if (popupPrefab != null && CurrentPopupAnchor != null)
        {
            MultiplierPopup popup = Instantiate(popupPrefab, CurrentPopupAnchor);
            popup.Setup(text, color);
        }
    }

    protected IEnumerator PlayCommonDamageAnimation(DamageReport report, SymbolData[,] grid, Color defaultColor)
    {
        // 사용된 빙고 칸 전체 하이라이트 (커지는 연출)
        if (CurrentSlotManager != null && grid != null)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (report.isUsedGrid[x, y])
                    {
                        if (grid[x, y].type != SymbolType.Bad)
                        {
                            CurrentSlotManager.PlaySymbolHighlight(new Vector2Int(x, y), grid[x, y].symbolSprite);
                        }
                    }
                }
            }
        }

        // 영수증 기반 팝업 순차 연출
        foreach (var log in report.logs)
        {
            ShowDamagePopup($"x{log.multiplier}!", defaultColor);
            yield return new WaitForSeconds(0.5f); // 도파민 뜸 들이기
        }
    }
}
