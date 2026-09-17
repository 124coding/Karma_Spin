using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("플레이어 영구 스탯")]
    public float baseDamage = 100f;
    public float lineMultiplier = 2f;
    public float clusterMultiplier = 5f;
    public float allMultiplier = 20f;
    public float taegeukMultiplier = 3f;
    public float badMultiplier = 0.5f;

    public void AddBaseDamage(float amount) { baseDamage += amount; }
}
