using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [SerializeField] int sceneIndex;

    // Hook this up to the button's OnClick
    public void GoToScene()
    {
        SceneLoader.Instance.LoadScene(sceneIndex);
    }
}