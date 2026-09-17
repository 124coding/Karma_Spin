using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

// 개별 당첨 내역 하나를 담는 로그 클래스
public class DamageLog
{
    public SymbolType elementType; // 어떤 속성이 터지고 있는지 (UI 색상이나 아이콘 변경에 사용)
    // public string message;         // "가로 빙고!", "약점 공략!
    public float multiplier;       // 화면에 "x2 빡!" 띄울 배율
    public float currentElementDamage; // '이 속성'이 지금까지 누적한 데미지 (합치기 전)

    public List<Vector2Int> hitPositions = new List<Vector2Int>();
}

// 턴마다 BattleManager에게 넘겨줄 최종 영수증
public class DamageReport
{
    public float finalDamage = 0f;                   // 최종 계산된 데미지
    public List<DamageLog> logs = new List<DamageLog>(); // 점진적 연출을 위한 로그 리스트
    public bool[,] isUsedGrid = new bool[3, 3];      // 활성화된 심볼 위치
    public bool hasWaterJackpot = false; // 수(Water) 속성 잭팟 여부
    public List<Vector2Int> meltedIceCoords = new List<Vector2Int>(); // 이번 계산으로 녹일 얼음 칸들

    // 태극 흉 잭팟 여부
    public bool isTaegeukJackpot = false;
    public bool isBadJackpot = false;
}

public static class DamageCalculator
{

    public static DamageReport CalculateTotalDamage(SymbolData[,] grid, BaseBattleManager battleManager)
    {
        DamageReport report = new DamageReport();

        float totalFinalDamage = 0f;

        HashSet<SymbolData> uniqueSymbols = new HashSet<SymbolData>();
        foreach (var symbol in grid)
        {
            if (symbol != null) uniqueSymbols.Add(symbol);
        }

        SymbolType targetWeakness = SymbolChart.GetWeakType(battleManager.TargetSymbolType);
        SymbolType targetResist = SymbolChart.GetStrongType(battleManager.TargetSymbolType);

        foreach (SymbolData symbolData in uniqueSymbols)
        {
            // 태극과 흉은 패스
            if (symbolData.type == SymbolType.Bad || symbolData.type == SymbolType.Taegeuk) continue;

            float elementMultiplier = 1f;
            bool hasMatch = false;
            int tCount = 0;

            void ApplyMatch(float multiplier, int taegeukCount, List<Vector2Int> hitPositions)
            {
                float finalMatchMultiplier = multiplier;

                if (taegeukCount > 0)
                {
                    float taegeukBonus = Mathf.Pow(battleManager.TaegeukMultiplier, taegeukCount);
                    finalMatchMultiplier *= taegeukBonus;
                }

                elementMultiplier *= finalMatchMultiplier;
                hasMatch = true;

                report.logs.Add(new DamageLog
                {
                    elementType = symbolData.type,
                    multiplier = finalMatchMultiplier,
                    currentElementDamage = battleManager.BaseDamage * elementMultiplier,
                    hitPositions = hitPositions
                });
            }

            for (int y = 0; y < 3; y++)
                if (CheckLine(symbolData.type, grid, 0, y, 1, y, 2, y, report, battleManager, out tCount)) ApplyMatch(battleManager.LineMultiplier, tCount, new List<Vector2Int> { new Vector2Int(0, y), new Vector2Int(1, y), new Vector2Int(2, y) });
            for (int x = 0; x < 3; x++)
                if (CheckLine(symbolData.type, grid, x, 0, x, 1, x, 2, report, battleManager, out tCount)) ApplyMatch(battleManager.LineMultiplier, tCount, new List<Vector2Int> { new Vector2Int(x, 0), new Vector2Int(x, 1), new Vector2Int(x, 2) });

            if (CheckLine(symbolData.type, grid, 0, 0, 1, 1, 2, 2, report, battleManager, out tCount)) ApplyMatch(battleManager.LineMultiplier, tCount, new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) });
            if (CheckLine(symbolData.type, grid, 0, 2, 1, 1, 2, 0, report, battleManager, out tCount)) ApplyMatch(battleManager.LineMultiplier, tCount, new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) });

            // 1. 3x3 전체 메가 잭팟
            if (CheckArea(symbolData.type, grid, 0, 0, 3, 3, report, battleManager, out tCount))
                        ApplyMatch(battleManager.AllMultiplier, tCount, new List<Vector2Int> {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)
            });

                    // 2. 2x3 클러스터 (좌측 2열)
                    if (CheckArea(symbolData.type, grid, 0, 0, 2, 3, report, battleManager, out tCount))
                        ApplyMatch(battleManager.ClusterMultiplier, tCount, new List<Vector2Int> {
                new Vector2Int(0, 0), new Vector2Int(1, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(0, 2), new Vector2Int(1, 2)
            });

                    // 3. 2x3 클러스터 (우측 2열)
                    if (CheckArea(symbolData.type, grid, 1, 0, 2, 3, report, battleManager, out tCount))
                        ApplyMatch(battleManager.ClusterMultiplier, tCount, new List<Vector2Int> {
                new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(1, 2), new Vector2Int(2, 2)
            });

                    // 4. 3x2 클러스터 (상단 2행)
                    if (CheckArea(symbolData.type, grid, 0, 0, 3, 2, report, battleManager, out tCount))
                        ApplyMatch(battleManager.ClusterMultiplier, tCount, new List<Vector2Int> {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
            });

                    // 5. 3x2 클러스터 (하단 2행)
                    if (CheckArea(symbolData.type, grid, 0, 1, 3, 2, report, battleManager, out tCount))
                        ApplyMatch(battleManager.ClusterMultiplier, tCount, new List<Vector2Int> {
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)
            });

            // 라인이나 덩어리가 하나라도 완성되었다면 기본 데미지에 곱해서 합산
            if (hasMatch)
            {
                if (symbolData.type == SymbolType.Water || symbolData.type == SymbolType.Taegeuk)
                {
                    report.hasWaterJackpot = true;
                }

                if (symbolData.type == targetWeakness)
                {
                    elementMultiplier *= 1.5f;
                    report.logs.Add(new DamageLog { elementType = symbolData.type, multiplier = 1.5f, currentElementDamage = battleManager.BaseDamage * elementMultiplier });
                }
                else if (symbolData.type == targetResist)
                {
                    elementMultiplier *= 0.5f;
                    report.logs.Add(new DamageLog { elementType = symbolData.type, multiplier = 0.5f, currentElementDamage = battleManager.BaseDamage * elementMultiplier });
                }

                // 속성 연산이 끝났으므로 전체 데미지에 더해줌
                totalFinalDamage += (battleManager.BaseDamage * elementMultiplier);
            }
        }

        int dummyCount = 0;

        // 태극 독립 잭팟 판정
        bool hasTaegeukJackpot = false;
        List<Vector2Int> taegeukHitPositions = new List<Vector2Int>(); // 추가된 리스트

        void CheckAndAddTaegeuk(bool success, params Vector2Int[] positions)
        {
            if (success) { hasTaegeukJackpot = true; taegeukHitPositions.AddRange(positions); }
        }

        // 가로 라인
        for (int y = 0; y < 3; y++) CheckAndAddTaegeuk(CheckLine(SymbolType.Taegeuk, grid, 0, y, 1, y, 2, y, report, battleManager, out dummyCount), new Vector2Int(0, y), new Vector2Int(1, y), new Vector2Int(2, y));
        // 세로 라인
        for (int x = 0; x < 3; x++) CheckAndAddTaegeuk(CheckLine(SymbolType.Taegeuk, grid, x, 0, x, 1, x, 2, report, battleManager, out dummyCount), new Vector2Int(x, 0), new Vector2Int(x, 1), new Vector2Int(x, 2));
        // 대각선
        CheckAndAddTaegeuk(CheckLine(SymbolType.Taegeuk, grid, 0, 0, 1, 1, 2, 2, report, battleManager, out dummyCount), new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2));
        CheckAndAddTaegeuk(CheckLine(SymbolType.Taegeuk, grid, 0, 2, 1, 1, 2, 0, report, battleManager, out dummyCount), new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0));

        // 3x3 에리어
        CheckAndAddTaegeuk(CheckArea(SymbolType.Taegeuk, grid, 0, 0, 3, 3, report, battleManager, out dummyCount),
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2));

        if (hasTaegeukJackpot)
        {
            report.isTaegeukJackpot = true;
            report.hasWaterJackpot = true;

            if (report.logs.Count == 0)
            {
                float taegeukDamage = battleManager.BaseDamage * 77f;
                totalFinalDamage = taegeukDamage;

                report.logs.Add(new DamageLog
                {
                    elementType = battleManager.TargetSymbolType,
                    multiplier = 77f,
                    currentElementDamage = taegeukDamage,
                    hitPositions = taegeukHitPositions
                });
            }
            else
            {
                totalFinalDamage *= 77f;
                report.logs.Add(new DamageLog
                {
                    elementType = SymbolType.Taegeuk,
                    multiplier = 77f,
                    currentElementDamage = totalFinalDamage,
                    hitPositions = taegeukHitPositions
                });
            }
        }

        // 흉 잭팟 독립 판정
        bool hasBadJackpot = false;
        List<Vector2Int> badHitPositions = new List<Vector2Int>(); // 추가된 리스트

        void CheckAndAddBad(bool success, params Vector2Int[] positions)
        {
            if (success) { hasBadJackpot = true; badHitPositions.AddRange(positions); }
        }

        for (int y = 0; y < 3; y++) CheckAndAddBad(CheckLine(SymbolType.Bad, grid, 0, y, 1, y, 2, y, report, battleManager, out dummyCount), new Vector2Int(0, y), new Vector2Int(1, y), new Vector2Int(2, y));
        for (int x = 0; x < 3; x++) CheckAndAddBad(CheckLine(SymbolType.Bad, grid, x, 0, x, 1, x, 2, report, battleManager, out dummyCount), new Vector2Int(x, 0), new Vector2Int(x, 1), new Vector2Int(x, 2));
        CheckAndAddBad(CheckLine(SymbolType.Bad, grid, 0, 0, 1, 1, 2, 2, report, battleManager, out dummyCount), new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2));
        CheckAndAddBad(CheckLine(SymbolType.Bad, grid, 0, 2, 1, 1, 2, 0, report, battleManager, out dummyCount), new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0));
        CheckAndAddBad(CheckArea(SymbolType.Bad, grid, 0, 0, 3, 3, report, battleManager, out dummyCount),
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2));

        if (hasBadJackpot)
        {
            report.isBadJackpot = true;
            totalFinalDamage = 0f;

            report.logs.Add(new DamageLog
            {
                elementType = SymbolType.Bad,
                multiplier = 0f,
                currentElementDamage = 0f,
                hitPositions = badHitPositions 
            });
        }

        // 최종 데미지 저장 후 영수증 반환
        report.finalDamage = totalFinalDamage;

        return report;
    }

    // 특정 속성이거나 태극 혹은 흉인지 판단
    private static bool IsValid(SymbolType targetType, SymbolData s, int x, int y, BaseBattleManager battleManager)
    {
        Vector2Int pos = new Vector2Int(x, y);

        bool? customValidity = battleManager.EvaluateCustomValidity(pos, targetType, s);
        if (customValidity.HasValue)
        {
            return customValidity.Value;
        }

        if (targetType == SymbolType.Bad)
        {
            return s.type == SymbolType.Bad;
        }

        return s.type == targetType || s.type == SymbolType.Taegeuk;
    }

    // 3칸 라인 판정
    private static bool CheckLine(SymbolType targetType, SymbolData[,] grid, int x1, int y1, int x2, int y2, int x3, int y3, DamageReport report, BaseBattleManager battleManager, out int taegeukCount)
    {
        taegeukCount = 0;

        if (IsValid(targetType, grid[x1, y1], x1, y1, battleManager) && IsValid(targetType, grid[x2, y2], x2, y2, battleManager) && IsValid(targetType, grid[x3, y3], x3, y3, battleManager))
        {
            report.isUsedGrid[x1, y1] = true;
            report.isUsedGrid[x2, y2] = true;
            report.isUsedGrid[x3, y3] = true;

            if (grid[x1, y1].type == SymbolType.Taegeuk) taegeukCount++;
            if (grid[x2, y2].type == SymbolType.Taegeuk) taegeukCount++;
            if (grid[x3, y3].type == SymbolType.Taegeuk) taegeukCount++;

            if (targetType == SymbolType.Fire)
            {
                report.meltedIceCoords.Add(new Vector2Int(x1, y1));
                report.meltedIceCoords.Add(new Vector2Int(x2, y2));
                report.meltedIceCoords.Add(new Vector2Int(x3, y3));
            }

            return true;
        }
        return false;
    }

    private static bool CheckArea(SymbolType targetType, SymbolData[,] grid, int startX, int startY, int width, int height, DamageReport report, BaseBattleManager battleManager, out int taegeukCount)
    {
        taegeukCount = 0;

        for (int x = startX; x < startX + width; x++) { for (int y = startY; y < startY + height; y++) { if (!IsValid(targetType, grid[x, y], x, y, battleManager)) return false; } }
        for (int x = startX; x < startX + width; x++) {
            for (int y = startY; y < startY + height; y++) {
                report.isUsedGrid[x, y] = true;

                if (grid[x, y].type == SymbolType.Taegeuk)
                    taegeukCount++;

                if (targetType == SymbolType.Fire)
                {
                    report.meltedIceCoords.Add(new Vector2Int(x, y));
                }
            } 
        }
        return true;
    }
}
