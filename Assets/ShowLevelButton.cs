using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowLevelButton : MonoBehaviour
{
    int level;
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.GetString("" + level, "Locked").Equals("Unlocked"))
        {
            Destroy(this.gameObject);
        }
    }
}
