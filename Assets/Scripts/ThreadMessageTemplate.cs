using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreadMessageTemplate : MonoBehaviour
{
    public Text m_TopicText;
    public Text m_TimestampText;

    public void SetData(string topic, string timestamp)
    {
        m_TopicText.text = topic;
        m_TimestampText.text = timestamp;
    }

	void Start ()
    {
		
	}
	
}
