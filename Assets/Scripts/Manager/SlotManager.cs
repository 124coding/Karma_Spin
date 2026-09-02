using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    public ReelController[] reels = new ReelController[3];
    public List<SymbolData> allAvailableSymbols;

    public int minStackSize;
    public int maxStackSize;

    public BattleManager battleManager;

    private int stoppedReelCount = 0;
    private SymbolData[,] currentGrid = new SymbolData[3, 3];

    // TODO: 각 릴 객체나 칸마다 기믹 상태(isFrozen, isBlinded)를 주입하고 관리하는 로직 추가 필요

<<<<<<< Updated upstream
    public void SpinAllReels()
=======
    // TODO: Test 삭제 필요
    private void Start()
    {
        SettingReels();
    }

    public void OnClickSpinButton()
    {
        if (isSpinning) return; // 이미 돌고 있으면 무시
        StartCoroutine(SpinSequenceRoutine());
    }

    private IEnumerator SpinSequenceRoutine()
    {
        if (isSpinning) yield break; // 이미 돌고 있으면 즉시 취소

        isSpinning = true;
        spinButton.interactable = false; // 버튼 비활성화 (시각적 처리)

        SpinAllReels();
        yield return null;
    }

    private void SpinAllReels()
>>>>>>> Stashed changes
    {
        stoppedReelCount = 0;

        int lockedIndex = -1;
        BossManager boss = battleManager.BossManager;

        if (boss.CurrentSymbol == SymbolType.Earth && boss.remainingEarthLockTurns > 0)
        {
            lockedIndex = boss.lockedReelIndex;
        }

        for (int i = 0; i < reels.Length; ++i)
        {
            int reelIndex = i;

            if(reelIndex == lockedIndex)
            {
                stoppedReelCount++;

                if (stoppedReelCount == 3)
                {
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

                // 3개의 릴이 모두 멈췄다면?
                if (stoppedReelCount == 3)
                {
                    Debug.Log("모든 릴 정지 완료! 데미지 정산을 시작합니다.");
                    battleManager.OnReelStopped(currentGrid);
                }
            }));
        }
    }

    public void SettingReels()
    {
        foreach(var reel in reels)
        {
            List<SymbolData> generatedStrip = GenerateStackedReelStrip();
            reel.InitializeReel(generatedStrip);
        }
    }

    private List<SymbolData> GenerateStackedReelStrip()
    {
        // 심볼들을 1 ~ 4개 단위의 덩어리로 만들어 저장
        List<List<SymbolData>> chunks = new List<List<SymbolData>>();

        foreach (var symbol in allAvailableSymbols)
        {
            int remainingCount = symbol.baseWeight;

            while (remainingCount > 0) {
                int stackSize = 1;

                if(symbol.type != SymbolType.Taegeuk && symbol.type != SymbolType.Bad)
                {
                    int roll1 = Random.Range(minStackSize, maxStackSize + 1);
                    int roll2 = Random.Range(minStackSize, maxStackSize + 1);
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
            int randomIndex = Random.Range(i, chunks.Count);
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

        return finalStrip;
    }

    public void UnlockSpinButton()
    {
        isSpinning = false;
        spinButton.interactable = true;
    }
}
