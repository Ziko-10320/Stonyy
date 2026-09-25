using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] string sceneToLoad;

    // Hook this up to the button's OnClick
    public void OnClicked()
    {
        SceneLoader.Instance.LoadScene(sceneToLoad);
    }
}