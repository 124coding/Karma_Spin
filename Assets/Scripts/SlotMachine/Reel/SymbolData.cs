using UnityEngine;

public enum SymbolType { Fire, Metal, Wood, Earth, Water, Taegeuk, Bad }

[CreateAssetMenu(fileName = "SymbolData", menuName = "Slot/SymbolData")]
public class SymbolData : ScriptableObject
{
    public SymbolType type;
    public Sprite symbolSprite;
    [Range(1, 100)] public int baseWeight; // 릴 내부 가중치
}