using UnityEngine;

public static class SaveManager
{
    // Отметить что-то как пройденное
    public static void SetCompleted(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // Проверить пройдено ли
    public static bool IsCompleted(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // Сбросить один прогресс (для тестов)
    public static void Reset(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    // Сбросить всё (для тестов или новой игры)
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
