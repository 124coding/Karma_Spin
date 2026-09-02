using System.Collections.Generic;
using UnityEngine;

public static class SymbolChart
{
    private static readonly Dictionary<SymbolType, SymbolType> Weaknesses = new Dictionary<SymbolType, SymbolType>
    {
        { SymbolType.Fire, SymbolType.Water },   // 불은 물에 약함
        { SymbolType.Water, SymbolType.Earth },  // 물은 흙에 약함
        { SymbolType.Earth, SymbolType.Wood },   // 흙은 나무에 약함
        { SymbolType.Wood, SymbolType.Metal },   // 나무는 금속에 약함
        { SymbolType.Metal, SymbolType.Fire }    // 금속은 불에 약함
    };

    // 특정 속성의 약점 속성을 반환하는 함수
    public static SymbolType GetWeakType(SymbolType type)
    {
        if (Weaknesses.TryGetValue(type, out SymbolType weakType))
        {
            return weakType;
        }

        // 흉이나 태극처럼 약점이 따로 없는 경우 처리
        return SymbolType.Bad; // 혹은 상황에 맞는 예외 값 반환
    }

    public static SymbolType GetStrongType(SymbolType type)
    {
        foreach (var pair in Weaknesses)
        {
            if (pair.Value == type) return pair.Key;
        }

        return SymbolType.Bad;
    }
}
