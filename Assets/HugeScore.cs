using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HugeScore : MonoBehaviour {
    private Text m_text;
	// Use this for initialization
	void Start () {
        m_text.text = "Game Over\nClick to continue\nHuge Score:" + PlayerPrefs.GetInt("HighScore") + "\nYour Score:" + PlayerPrefs.GetInt("YourScore");
    }
}
