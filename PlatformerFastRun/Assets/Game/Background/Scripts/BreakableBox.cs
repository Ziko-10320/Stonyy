using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [SerializeField] GameObject breakEffect;

    public void Break()
    {
        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}