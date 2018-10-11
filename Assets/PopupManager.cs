using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour {

    public struct popupInfo
    {
        public string title;
        public string content;
    };

    public static PopupManager current;
    public PopupScript popup;

    public List<popupInfo> popupQueue;
    bool queueRunning = false;

    public void MakePopup(string title, string content)
    {
        popupInfo newPopupInfo = new popupInfo
        {
            title = title,
            content = content
        };
        popupQueue.Add(newPopupInfo);
        if (!queueRunning)
        {
            queueRunning = true;
            StartCoroutine(startShowingPopups());
        }
    }

    IEnumerator startShowingPopups()
    {
        while (popupQueue.Count > 0)
        {
            popup.NewPopup(popupQueue[0].title, popupQueue[0].content);
            yield return null;
            popupQueue.RemoveAt(0);
            Debug.Log("Showing popup");
            if (popupQueue.Count == 0)
            {
                Debug.Log("Stopping now");
                yield return StartCoroutine(fadeSequence());
                queueRunning = false;
                popup.gameObject.SetActive(false);
                break;
            }
            yield return StartCoroutine(fadeSequence());
            popup.gameObject.SetActive(false);
        }
    }

    IEnumerator fadeSequence()
    {
        float waitTime = 5f;
        while (waitTime > 0f && popup.gameObject.activeSelf)
        {
            waitTime -= Time.deltaTime;
            yield return null;
        }
    }

    private void Awake()
    {
        current = this;
    }


    // Use this for initialization
    void Start () {
        popupQueue = new List<popupInfo>();
        //MakePopup("testing", "123");
        //MakePopup("testing", "456");
        //MakePopup("testing", "789");
    }
	
}
