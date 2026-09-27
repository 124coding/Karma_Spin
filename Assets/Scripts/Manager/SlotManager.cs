using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SlotGimmickState
{
    Normal = 0,
    Earth_Locked = 1,
    Water_Frozen = 2,
    Wood_Corrupted = 3,
    Fire_Burned = 4
}

public class SlotManager : MonoBehaviour
{
    [Header("동기화 난수")]
    public int gimmickSeed = 0;

    public ReelController[] reels = new ReelController[3];
    public List<SymbolData> allAvailableSymbols;

    [Header("슬롯 전체 상태")]
    public SlotGimmickState currentState { get; private set; } = SlotGimmickState.Normal;

    // 현재 기믹이 활용할 세부 데이터 (릴 번호 또는 좌표들)
    public int activeReelIndex = -1;
    public List<Vector2Int> activeCells = new List<Vector2Int>();
    public int pendingActionCount = 0;

    public void AddActiveCell(Vector2Int pos)
    {
        if (!activeCells.Contains(pos))
        {
            activeCells.Add(pos);

            // TODO: currentState에 따라 pos 좌표에 맞는 연출 키기
        }
    }

    // 새로운 기믹을 슬롯에 부여하려고 시도하는 공통 함수
    public bool TryApplyGimmick(SlotGimmickState newState)
    {
        if (currentState != SlotGimmickState.Normal && currentState != newState)
        {
            Debug.Log($"[방어됨] 슬롯이 이미 {currentState} 상태이므로 {newState} 기믹을 무시합니다!");
            return false;
        }

        currentState = newState;
        Debug.Log($"[기믹 발동] 슬롯 머신 전체가 {newState} 상태로 변환되었습니다!");
        return true;
    }

    // 슬롯을 정상으로 되돌리는 정화 함수
    public void ClearGimmick()
    {
        currentState = SlotGimmickState.Normal;
        activeReelIndex = -1;
        activeCells.Clear();
        // TODO: 슬롯판 전체를 덮고 있던 기믹 연출 끄기
    }
    public bool IsCellAffected(Vector2Int pos) => activeCells.Contains(pos);


    public void RemoveCellEffect(Vector2Int pos)
    {
        if (activeCells.Contains(pos))
        {
            activeCells.Remove(pos);
            // TODO: 해당 칸 연출(얼음, 덩굴 등) 끄기

            if (activeCells.Count == 0 && currentState != SlotGimmickState.Earth_Locked && pendingActionCount == 0)
            {
                ClearGimmick();
            }
        }
    }

    public void SpreadCorruption()
    {
        if (currentState != SlotGimmickState.Wood_Corrupted || activeCells.Count == 0 || activeCells.Count >= 9) return;

        List<Vector2Int> availableNeighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

        foreach(var slot in activeCells)
        {
            foreach (var dir in directions)
            {
                Vector2Int neighbor = slot + dir;
                if(neighbor.x >= 0 && neighbor.x < 3 && neighbor.y >= 0 && neighbor.y < 3)
                {
                    if(!activeCells.Contains(neighbor) && !availableNeighbors.Contains(neighbor)) availableNeighbors.Add(neighbor);
                }
            }
        }

        System.Random rng = new System.Random(gimmickSeed);

        if (availableNeighbors.Count > 0)
        {
            AddActiveCell(availableNeighbors[rng.Next(0, availableNeighbors.Count)]);
        }
    }

    public void ProcessPendingFires()
    {
        if (currentState != SlotGimmickState.Fire_Burned || pendingActionCount <= 0) return;

        List<Vector2Int> available = new List<Vector2Int>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (!activeCells.Contains(new Vector2Int(x, y))) available.Add(new Vector2Int(x, y));
            }
        }

        System.Random rng = new System.Random(gimmickSeed);

        for (int i = 0; i < pendingActionCount; i++)
        {
            if (available.Count == 0) break;

            int rand = rng.Next(0, available.Count);

            AddActiveCell(available[rand]);
            available.RemoveAt(rand);
            // TODO: 불타는 UI 추가
        }
        pendingActionCount = 0;
    }

    public int minStackSize;
    public int maxStackSize;

    private bool isSpinning = false;
    public Button spinButton;

    public BaseBattleManager battleManager;

    private int stoppedReelCount = 0;
    private SymbolData[,] currentGrid = new SymbolData[3, 3];

    [Header("클론 하이라이트 연출")]
    public SymbolHighlight highlightClonePrefab;
    public Transform[] slotAnchors = new Transform[9];

    private Dictionary<SymbolType, int> extraSymbolWeights = new Dictionary<SymbolType, int>();

    [Header("1턴 한정 릴 조작 버프")]
    public Dictionary<SymbolType, SymbolType> tempReplacements = new Dictionary<SymbolType, SymbolType>();
    public Dictionary<SymbolType, int> tempExtraWeights = new Dictionary<SymbolType, int>();

    public void AddTempReplacement(SymbolType from, SymbolType to)
    {
        tempReplacements[from] = to;
    }

    public void AddTempExtraWeights(SymbolType type, int amount)
    {
        if (tempExtraWeights.ContainsKey(type)) tempExtraWeights[type] += amount;
        else tempExtraWeights[type] = amount;
    }

    public void ResetTempReelBuffs(int syncSeed)
    {
        if (tempReplacements.Count > 0 || tempExtraWeights.Count > 0)
        {
            tempReplacements.Clear();
            tempExtraWeights.Clear();
            SettingReels(syncSeed);
        }
    }
    public void InitializeNetworkGame(int syncSeed)
    {
        SettingReels(syncSeed); // 네트워크 공통 시드가 맞춰진 직후에 릴 생성
    }

    public void ExecuteNetworkSpinSequence()
    {
        if (isSpinning) return;
        StartCoroutine(SpinSequenceRoutine());
    }

    private IEnumerator SpinSequenceRoutine()
    {
        if (isSpinning) yield break; // 이미 돌고 있으면 즉시 취소

        isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        SpinAllReels();
        yield return null;
    }

    private void SpinAllReels()
    {
        stoppedReelCount = 0;

        int lockedIndex = (currentState == SlotGimmickState.Earth_Locked) ? activeReelIndex : -1;

        for (int i = 0; i < reels.Length; ++i)
        {
            int reelIndex = i;

            if(reelIndex == lockedIndex)
            {
                stoppedReelCount++;

                if (stoppedReelCount == 3)
                {
                    ProcessPendingFires();

                    Debug.Log("모든 릴 정지 완료! 데미지 정산을 시작합니다.");
                    battleManager.OnReelStopped(currentGrid);
                }

                continue;
            }

            StartCoroutine(reels[i].Spin(results =>
            {
                currentGrid[reelIndex, 0] = results[0];
                currentGrid[reelIndex, 1] = results[1];
                currentGrid[reelIndex, 2] = results[2];

                stoppedReelCount++;

                CheckAllReelsStopped();
            }));
        }
    }

    private void CheckAllReelsStopped()
    {
        if (stoppedReelCount == 3)
        {
            ProcessPendingFires(); // 화염(Fire) 예고가 있다면 지금 발동
            Debug.Log("모든 릴 정지 완료! 데미지 정산을 시작합니다.");
            battleManager.OnReelStopped(currentGrid);
        }
    }

    public void UnlockSpinButton()
    {
        isSpinning = false;
        if (spinButton != null) spinButton.interactable = true;
    }

    public void SettingReels(int syncSeed)
    {
        System.Random reelRng = new System.Random(syncSeed);

        foreach (var reel in reels)
        {
            List<SymbolData> generatedStrip = GenerateStackedReelStrip(reelRng);
            reel.InitializeReel(generatedStrip);
        }
    }

    private List<SymbolData> GenerateStackedReelStrip(System.Random rng)
    {
        // 심볼들을 1 ~ 4개 단위의 덩어리로 만들어 저장
        List<List<SymbolData>> chunks = new List<List<SymbolData>>();

        foreach (var symbol in allAvailableSymbols)
        {
            int bonus = extraSymbolWeights.ContainsKey(symbol.type) ? extraSymbolWeights[symbol.type] : 0;
            int tempBonus = tempExtraWeights.ContainsKey(symbol.type) ? tempExtraWeights[symbol.type] : 0;
            int remainingCount = symbol.baseWeight + bonus + tempBonus;

            while (remainingCount > 0) {
                int stackSize = 1;

                if(symbol.type != SymbolType.Taegeuk && symbol.type != SymbolType.Bad)
                {
                    int roll1 = rng.Next(minStackSize, maxStackSize + 1);
                    int roll2 = rng.Next(minStackSize, maxStackSize + 1);
                    stackSize = Mathf.Min(roll1, roll2);
                    if (stackSize > remainingCount) stackSize = remainingCount;
                }

                List<SymbolData> chunk = new List<SymbolData>();
                for(int i = 0; i < stackSize; i++)
                {
                    chunk.Add(symbol);
                }

                chunks.Add(chunk);
                remainingCount -= stackSize;
            }
        }

        // 덩어리 단위로 무작위 셔플
        for(int i = 0; i < chunks.Count; i++)
        {
            int randomIndex = rng.Next(i, chunks.Count);
            List<SymbolData> temp = chunks[i];
            chunks[i] = chunks[randomIndex];
            chunks[randomIndex] = temp;
        }

        for(int i = 0; i < chunks.Count - 1; i++)
        {
            if (chunks[i][0].type == chunks[i + 1][0].type)
            {
                for(int j = i + 2; j < chunks.Count; j++)
                {
                    if (chunks[j][0].type != chunks[i][0].type)
                    {
                        var temp = chunks[i + 1];
                        chunks[i + 1] = chunks[j];
                        chunks[j] = temp;
                        break;
                    }
                }
            }
        }

        // 섞인 덩어리 길게 이어 붙이기
        List<SymbolData> finalStrip = new List<SymbolData>();
        foreach (var chunk in chunks)
        {
            finalStrip.AddRange(chunk);
        }

        if(tempReplacements.Count > 0)
        {
            for(int i = 0; i < finalStrip.Count; i++)
            {
                if (tempReplacements.TryGetValue(finalStrip[i].type, out SymbolType targetType))
                {
                    SymbolData replacementData = allAvailableSymbols.Find(s => s.type == targetType);
                    if (replacementData != null)
                    {
                        finalStrip[i] = replacementData;
                    }
                }
            }
        }

        return finalStrip;
    }

    public void AddExtraWeight(SymbolType type, int amount)
    {
        if (extraSymbolWeights.ContainsKey(type))
        {
            extraSymbolWeights[type] += amount;
        }
        else
        {
            extraSymbolWeights[type] = amount;
        }
    }

    public void PlaySymbolHighlight(Vector2Int pos, Sprite symbolSprite)
    {
        int index = pos.x * 3 + pos.y;

        if(index >= 0 && index < slotAnchors.Length && slotAnchors[index] != null)
        {
            SymbolHighlight clone = Instantiate(highlightClonePrefab, slotAnchors[index]);
            clone.Setup(symbolSprite);
        }
    }
}
