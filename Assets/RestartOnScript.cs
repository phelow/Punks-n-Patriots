using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartOnScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public void ButtonPress()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);

    }
}
