using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadScore : MonoBehaviour {
    [SerializeField]
    private Text m_scoreText;
    [SerializeField]
    private Text m_highScoreText;

	// Use this for initialization
	void Start () {
        m_scoreText.text = ""+PlayerPrefs.GetInt("YourScore",0);
        m_highScoreText.text = "" + PlayerPrefs.GetInt("HighScore",0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
