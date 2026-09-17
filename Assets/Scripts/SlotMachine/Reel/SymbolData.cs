using UnityEngine;

public enum SymbolType { 
    None = 0,
    Fire = 1, Metal = 2, Wood = 3, Earth = 4, Water = 5, Taegeuk = 6, Bad = 7 }

[CreateAssetMenu(fileName = "SymbolData", menuName = "Slot/SymbolData")]
public class SymbolData : ScriptableObject
{
    public SymbolType type;
    public Sprite symbolSprite;
    [Range(1, 100)] public int baseWeight; // 릴 내부 가중치
}