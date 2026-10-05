using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] string levelId;   // "0", "0.3", "5", "Boss", etc. — must match what you used in LevelSelectManager
    [Header("State Colors")]
    [SerializeField] Color lockedColor = Color.gray;
    [SerializeField] Color unlockedColor = Color.white;
    [SerializeField] Color completedColor = Color.green;
    [SerializeField] Image targetImage;
    [SerializeField] GameObject lockIcon; // optional visual, shown when locked
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = LevelSelectManager.Instance.IsUnlocked(levelId);
        bool completed = LevelProgress.IsCompleted(levelId);

        button.interactable = unlocked;
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (targetImage != null)
        {
            if (!unlocked)
                targetImage.color = lockedColor;
            else if (completed)
                targetImage.color = completedColor;
            else
                targetImage.color = unlockedColor;
        }
    }
}