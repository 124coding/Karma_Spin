using UnityEngine;

// 개별 아이템 기능
public abstract class ItemEffect : ScriptableObject
{
    public virtual bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report) { return true; }
}
