using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeOnLeftClick : MonoBehaviour {
    [SerializeField]
    private Text m_text;

	// Use this for initialization
	void Start () {
        StartCoroutine(FadeOnClick());
	}

    private IEnumerator FadeOnClick()
    {
        yield return new WaitForSeconds(3.0f);

        while (!Input.GetMouseButton(0))
        {
            yield return new WaitForEndOfFrame();
        }

        float t = 0.0f;
        while(t < 1.0f)
        {
            m_text.color = Color.Lerp(Color.white, Color.clear, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
	
}
