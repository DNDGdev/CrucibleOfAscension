using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Predefined Characters")]
    public CharacterData[] availableCharacters;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    public GameConfig gameConfig;


    public CharacterData GetCharacterForPlayer(int playerIndex)
    {
        return availableCharacters[playerIndex % availableCharacters.Length];
    }
}
