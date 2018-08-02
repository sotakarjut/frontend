using UnityEngine;

public class UIScreen : MonoBehaviour
{
	void Start ()
    {
	}

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
