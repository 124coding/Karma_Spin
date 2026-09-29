using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("얼음 연쇄 파훼용 데이터")]
    public bool isDummyCalculation = false; // 현재 계산이 미리보기(가계산) 중인지
    public HashSet<Vector2Int> thawedIceCoordsThisTurn = new HashSet<Vector2Int>(); // 이번 스핀에 깨질 얼음 좌표들

    public bool? EvaluateCustomValidity(Vector2Int pos, SymbolType targetType, SymbolData s)
    {
        if (CurrentSlotManager == null || CurrentSlotManager.currentState == SlotGimmickState.Normal) return null;

        bool isAffected = CurrentSlotManager.IsCellAffected(pos);

        switch(CurrentSlotManager.currentState)
        {
            case SlotGimmickState.Water_Frozen:
                if (isAffected)
                {
                    if (!isDummyCalculation && thawedIceCoordsThisTurn.Contains(pos))
                    {
                        return null; // null을 반환하면 계산기가 알아서 태극/속성 판정을 해줍니다.
                    }

                    if (targetType == SymbolType.Fire)
                        return s.type == SymbolType.Fire || s.type == SymbolType.Taegeuk;
                    if (targetType == SymbolType.Wood)
                        return s.type == SymbolType.Wood || s.type == SymbolType.Taegeuk;
                    if (targetType == SymbolType.Bad)
                        return true;

                    return false;
                }
                break;
            case SlotGimmickState.Wood_Corrupted:
                if (isAffected)
                {
                    if (s.type == SymbolType.Fire || s.type == SymbolType.Earth || s.type == SymbolType.Taegeuk)
                    {
                        return null;
                    }

                    if (targetType == SymbolType.Bad) return true;
                    return false;
                }
                break;

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

    protected void ProcessElementReactions(SymbolData[,] grid, DamageReport report)
    {
        if (CurrentSlotManager == null || CurrentSlotManager.currentState == SlotGimmickState.Normal) return;
        if (CurrentSlotManager.activeCells.Count == 0 && CurrentSlotManager.currentState != SlotGimmickState.Fire_Burned) return;

        SlotGimmickState state = CurrentSlotManager.currentState;

        // 화염(Fire_Burned) 상호작용: 수(Water) 잭팟이 터지면 불이 꺼짐
        if (state == SlotGimmickState.Fire_Burned)
        {
            if (report.hasWaterJackpot)
            {
                Debug.Log("<color=blue>[상호작용] 수(Water) 잭팟이 터져 화염이 모두 진화되었습니다!</color>");
                CurrentSlotManager.pendingActionCount = 0;
                CurrentSlotManager.ClearFirePositions();
                CurrentSlotManager.ClearGimmick();
            }
        }

        // 잠식(Wood_Corrupted) & 빙결(Water_Frozen) 타일 상호작용
        for (int i = CurrentSlotManager.activeCells.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = CurrentSlotManager.activeCells[i];
            SymbolType landedType = grid[pos.x, pos.y].type;

            if (state == SlotGimmickState.Wood_Corrupted)
            {
                // [잠식 파훼] 불, 대지, 태극이 떨어지면 오염 정화
                if (landedType == SymbolType.Fire || landedType == SymbolType.Earth || landedType == SymbolType.Taegeuk)
                {
                    Debug.Log($"<color=green>[상호작용] {landedType} 속성으로 덩굴을 태웠습니다!</color>");
                    CurrentSlotManager.RemoveCellEffect(pos);
                }
                // [잠식 악화] 물이 떨어지면 오염 확산
                else if (landedType == SymbolType.Water)
                {
                    CurrentSlotManager.SpreadCorruption();
                }
            }
            else if (state == SlotGimmickState.Water_Frozen)
            {
                // [빙결 파훼] 해당 칸을 포함하는 불(Fire) 또는 목(Wood) 빙고가 터졌는지 확인
                bool isIceBroken = false;

                foreach (var log in report.logs)
                {
                    // 이 빙고(잭팟) 영역에 얼어붙은 칸(pos)이 포함되어 있다면
                    if (log.hitPositions != null && log.hitPositions.Contains(pos))
                    {
                        // 빙고에 포함된 칸들 중 하나라도 화염이나 목 속성이 있는지 확인 (태극이 섞여도 판정 가능)
                        bool isFireOrWoodBingo = false;
                        foreach (Vector2Int hitPos in log.hitPositions)
                        {
                            SymbolType hitType = grid[hitPos.x, hitPos.y].type;
                            if (hitType == SymbolType.Fire || hitType == SymbolType.Wood)
                            {
                                isFireOrWoodBingo = true;
                                break; // 하나라도 확인되면 해당 빙고는 화염/목 속성 빙고로 인정
                            }
                        }

                        if (isFireOrWoodBingo)
                        {
                            isIceBroken = true;
                            break;
                        }
                    }
                }

                if (isIceBroken)
                {
                    Debug.Log($"<color=cyan>[상호작용] 화염/목 속성 빙고가 폭발하여 ({pos.x}, {pos.y})의 얼음이 산산조각 났습니다!</color>");
                    CurrentSlotManager.RemoveCellEffect(pos);
                }
            }
        }
    }

    public virtual float GetTempSymbolMultiplier(SymbolType type) { return 1f; }
}
