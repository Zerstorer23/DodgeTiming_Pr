using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigsManager : MonoBehaviour
{
    public UnitConfig[] unitConfigs;
    public BuffConfig[] buffConfigs;
    public GameModeConfig[] gameModeConfigs;
    public static Dictionary<CharacterType, UnitConfig> unitDictionary;
    public static Dictionary<GameMode, GameModeConfig> gameModeDictionary;
    public static ConfigsManager instance;
    private void Awake()
    {
        instance = this;
        unitDictionary = new Dictionary<CharacterType, UnitConfig>();
        foreach (UnitConfig u in unitConfigs)
        {
            unitDictionary.Add(u.characterID, u);
        }

        gameModeDictionary = new Dictionary<GameMode, GameModeConfig>();
        foreach (GameModeConfig u in gameModeConfigs)
        {
            gameModeDictionary.Add(u.gameMode, u);
        }
        GameSession.gameModeInfo = gameModeDictionary[(GameMode)0];
    }
    public static CharacterType GetRandomCharacter()
    {
        int rand = Random.Range(1, instance.unitConfigs.Length);
        while (instance.unitConfigs[rand].noRandom) {
            rand = Random.Range(1, instance.unitConfigs.Length);
        }
        return instance.unitConfigs[rand].characterID;
    }
}
