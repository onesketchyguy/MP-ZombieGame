using UnityEngine;

public class Activator : MonoBehaviour
{
    public GameObject[] objects = null;
    [SerializeField] private bool activeOnStart = false;

    public void Start() => SetItems(activeOnStart);

    public void SetItems(bool value)
    {
        foreach (var item in objects) item.SetActive(value);
    }

    public void SetItemsActive() => SetItems(true);
    public void SetItemsInactive() => SetItems(false);
}
