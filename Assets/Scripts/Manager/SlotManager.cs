using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    public ReelController[] reels = new ReelController[3];
    public List<SymbolData> allAvailableSymbols;

    public int minStackSize;
    public int maxStackSize;

    // TODO: 각 릴 객체나 칸마다 기믹 상태(isFrozen, isBlinded)를 주입하고 관리하는 로직 추가 필요

    public void SpinAllReels()
    {
        foreach(var reel in reels)
        {
            StartCoroutine(reel.Spin(results =>
            {
                Debug.Log($"릴 정지! 결과: {results[0].type}, {results[1].type}, {results[2].type}");
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
                    stackSize = Random.Range(minStackSize, maxStackSize + 1);
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
}
