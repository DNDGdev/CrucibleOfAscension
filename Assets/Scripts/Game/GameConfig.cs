using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int maxPlayers = 3;
    public float matchDuration = 300f; // 5 minutes
    public float respawnTime = 3f;
    public int pointsPerKill = 1;
}