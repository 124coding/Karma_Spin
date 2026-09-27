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

    public abstract SymbolType TargetSymbolType { get; }

    public bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        if (CurrentSlotManager == null || CurrentSlotManager.currentState == SlotGimmickState.Normal) return null;

        bool isAffected = CurrentSlotManager.IsCellAffected(pos);

        switch(CurrentSlotManager.currentState)
        {
            case SlotGimmickState.Water_Frozen:
                if (isAffected)
                {
                    if(targetType == SymbolType.Fire) return s.type == SymbolType.Fire || targetType == SymbolType.Taegeuk;
                    if (targetType == SymbolType.Bad) return true;
                }
                break;
            case SlotGimmickState.Wood_Corrupted:
            case SlotGimmickState.Fire_Burned:
                if (isAffected && targetType == SymbolType.Bad) return true;
                if (isAffected) return false;
                break;
        }

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
        if (report.logs.Count == 0) yield break;

        // 영수증(로그)을 한 줄씩 꺼내보며 순차적으로 연출 진행
        foreach (var log in report.logs)
        {
            if (CurrentSlotManager != null && grid != null && log.hitPositions != null)
            {
                foreach (Vector2Int pos in log.hitPositions)
                {
                    if (grid[pos.x, pos.y].type != SymbolType.Bad)
                    {
                        CurrentSlotManager.PlaySymbolHighlight(pos, grid[pos.x, pos.y].symbolSprite);
                    }
                }
            }

            ShowDamagePopup($"x{log.multiplier}!", defaultColor);

            yield return new WaitForSeconds(1f);
        }
    }

    protected void ProcessWoodElementReactions(SymbolData[,] grid)
    {
        if (CurrentSlotManager == null || CurrentSlotManager.currentState != SlotGimmickState.Wood_Corrupted) return;
        if (CurrentSlotManager.activeCells.Count == 0) return;

        for (int i = CurrentSlotManager.activeCells.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = CurrentSlotManager.activeCells[i];
            SymbolType landedType = grid[pos.x, pos.y].type;

            // 정화 상호작용
            if(landedType == SymbolType.Fire || landedType == SymbolType.Earth || landedType == SymbolType.Taegeuk)
            {
                CurrentSlotManager.RemoveCellEffect(pos);
            }
            else if (landedType == SymbolType.Water)
            {
                CurrentSlotManager.SpreadCorruption();
            }
        }
    }

    public virtual float GetTempSymbolMultiplier(SymbolType type) { return 1f; }
}
