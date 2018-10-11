using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupScript : MonoBehaviour {
    public TextMeshProUGUI UItitle;
    public TextMeshProUGUI UIcontent;
    
    public void NewPopup(string title, string content)
    {
        gameObject.SetActive(false);
        UItitle.text = title;
        UIcontent.text = content;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
    }
}
