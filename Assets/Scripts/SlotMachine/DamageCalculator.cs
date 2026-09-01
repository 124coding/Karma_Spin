using System.Collections.Generic;
using UnityEngine;

// 개별 당첨 내역 하나를 담는 로그 클래스
public class DamageLog
{
    public float multiplier;   // 적용된 배율
}

// 턴마다 BattleManager에게 넘겨줄 최종 영수증
public class DamageReport
{
    public float finalDamage = 0f;                   // 최종 계산된 데미지
    public List<DamageLog> logs = new List<DamageLog>(); // 점진적 연출을 위한 로그 리스트
    public bool[,] isUsedGrid = new bool[3, 3];      // 활성화된 심볼 위치
}

public static class DamageCalculator
{
    private static readonly SymbolType[] Elements = {
        SymbolType.Fire, SymbolType.Water, SymbolType.Wood, SymbolType.Earth, SymbolType.Metal, SymbolType.Bad
    };

    public static DamageReport CalculateTotalDamage(SymbolData[,] grid, BattleManager battleManager)
    {
        DamageReport report = new DamageReport();
        float totalFinalDamage = 0f;

        foreach (var element in Elements)
        {
            float elementMultiplier = 1f;
            bool hasMatch = false;

            void ApplyMatch(float multiplier)
            {
                report.logs.Add(new DamageLog { multiplier = multiplier });
                elementMultiplier *= multiplier;
                hasMatch = true;
            }

            for (int y = 0; y < 3; y++)
            {
                if (CheckLine(element, grid, 0, y, 1, y, 2, y, report.isUsedGrid)) ApplyMatch(battleManager.LineMultiplier);
            }

            for (int x = 0; x < 3; x++)
            {
                if (CheckLine(element, grid, x, 0, x, 1, x, 2, report.isUsedGrid)) ApplyMatch(battleManager.LineMultiplier);
            }

            if (CheckLine(element, grid, 0, 0, 1, 1, 2, 2, report.isUsedGrid)) ApplyMatch(battleManager.LineMultiplier);
            if (CheckLine(element, grid, 0, 2, 1, 1, 2, 0, report.isUsedGrid)) ApplyMatch(battleManager.LineMultiplier);

            // 덩어리(Cluster) 잭팟 판정도 동일하게 수정
            if (CheckArea(element, grid, 0, 0, 3, 3, report.isUsedGrid)) ApplyMatch(battleManager.AllMultiplier);

            if (CheckArea(element, grid, 0, 0, 2, 3, report.isUsedGrid)) ApplyMatch(battleManager.ClusterMultiplier);
            if (CheckArea(element, grid, 1, 0, 2, 3, report.isUsedGrid)) ApplyMatch(battleManager.ClusterMultiplier);

            if (CheckArea(element, grid, 0, 0, 3, 2, report.isUsedGrid)) ApplyMatch(battleManager.ClusterMultiplier);
            if (CheckArea(element, grid, 0, 1, 3, 2, report.isUsedGrid)) ApplyMatch(battleManager.ClusterMultiplier);

            // 라인이나 덩어리가 하나라도 완성되었다면 기본 데미지에 곱해서 합산
            if (hasMatch)
            {
                totalFinalDamage += battleManager.BaseDamage * elementMultiplier;
            }
        }

        int badCount = 0;
        int taegeukCount = 0;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (report.isUsedGrid[x, y])
                {
                    if (grid[x, y].type == SymbolType.Bad) badCount++;
                    if (grid[x, y].type == SymbolType.Taegeuk) taegeukCount++;
                }
            }
        }

        if (taegeukCount > 0)
        {
            float taegeukMult = Mathf.Pow(battleManager.TaegeukMultiplier, taegeukCount);
            report.logs.Add(new DamageLog { multiplier = taegeukMult });
            totalFinalDamage *= taegeukMult;
        }

        if (badCount > 0)
        {
            float badMult = Mathf.Pow(battleManager.BadMultiplier, badCount);
            report.logs.Add(new DamageLog { multiplier = badMult });
            totalFinalDamage *= badMult;
        }

        // 최종 데미지 저장 후 영수증 반환
        report.finalDamage = totalFinalDamage;

        return report;
    }

    // 특정 속성이거나 태극 혹은 흉인지 판단
    private static bool IsValid(SymbolType targetType, SymbolData s)
    {
        return s.type == targetType || s.type == SymbolType.Taegeuk || s.type == SymbolType.Bad;
    }

    // 3칸 라인 판정
    private static bool CheckLine(SymbolType targetType, SymbolData[,] grid, int x1, int y1, int x2, int y2, int x3, int y3, bool[,] isUsed)
    {
        if (IsValid(targetType, grid[x1, y1]) && IsValid(targetType, grid[x2, y2]) && IsValid(targetType, grid[x3, y3]))
        {
            isUsed[x1, y1] = true; isUsed[x2, y2] = true; isUsed[x3, y3] = true; return true;
        }
        return false;
    }

    private static bool CheckArea(SymbolType targetType, SymbolData[,] grid, int startX, int startY, int width, int height, bool[,] isUsed)
    {
        for (int x = startX; x < startX + width; x++) { for (int y = startY; y < startY + height; y++) { if (!IsValid(targetType, grid[x, y])) return false; } }
        for (int x = startX; x < startX + width; x++) { for (int y = startY; y < startY + height; y++) { isUsed[x, y] = true; } }
        return true;
    }
}
