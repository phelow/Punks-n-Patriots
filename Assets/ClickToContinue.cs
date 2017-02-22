using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToContinue : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.anyKey)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(PlayerPrefs.GetString("LastLevel"));
        }
	}
}
