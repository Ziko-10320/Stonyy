using UnityEngine;

public static class LevelProgress
{
    public static bool IsCompleted(string levelId)
    {
        return PlayerPrefs.GetInt("Level_" + levelId + "_Completed", 0) == 1;
    }

    public static void SetCompleted(string levelId)
    {
        PlayerPrefs.SetInt("Level_" + levelId + "_Completed", 1);
        PlayerPrefs.Save();
    }
}