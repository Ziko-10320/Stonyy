using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance;

    [System.Serializable]
    public class LevelRow
    {
        public string rowName;          // just for your own organization in the inspector
        public string[] levelIds;       // e.g. ["0", "0.3", "0.7"]
        public bool sequentialWithinRow; // true = unlock one-by-one inside this row
    }

    public LevelRow[] rows;

    void Awake()
    {
        Instance = this;
    }

    public bool IsUnlocked(string levelId)
    {
        for (int r = 0; r < rows.Length; r++)
        {
            int index = System.Array.IndexOf(rows[r].levelIds, levelId);
            if (index == -1) continue; // not in this row, keep searching

            bool previousRowComplete = (r == 0) || AllCompleted(rows[r - 1]);

            if (!rows[r].sequentialWithinRow)
                return previousRowComplete; // whole row opens together

            // sequential row: first entry needs previous row done,
            // every entry after needs the one before it done
            if (index == 0) return previousRowComplete;
            return previousRowComplete && LevelProgress.IsCompleted(rows[r].levelIds[index - 1]);
        }

        Debug.LogWarning("LevelSelectManager: levelId not found in any row: " + levelId);
        return false;
    }

    bool AllCompleted(LevelRow row)
    {
        foreach (string id in row.levelIds)
            if (!LevelProgress.IsCompleted(id))
                return false;
        return true;
    }
}