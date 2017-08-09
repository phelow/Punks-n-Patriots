using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutOnRightClick : MonoBehaviour
{
    [SerializeField]
    private Text m_text;

    bool started = false;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(FadeOnClick());
        StartCoroutine(WaitToStart());
    }

    private IEnumerator WaitToStart()
    {
        while (!Input.GetMouseButton(1))
        {
            yield return new WaitForEndOfFrame();
        }
        started = true;
    }

    private IEnumerator FadeOnClick()
    {
        for(int i = 0; i < 10; i++)
        {
            m_text.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            yield return new WaitForSeconds(.3f);
        }

        m_text.color = Color.white;

        yield return new WaitForSeconds(3.0f);

        float timeWasted = 0.0f;

        while (started == false)
        {
            while(timeWasted < 0.0f && started == false)
            {
                timeWasted += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            m_text.color = Color.clear;

            yield return new WaitForSeconds(1.0f);
            m_text.color = Color.white;
            yield return new WaitForSeconds(1.0f);

            yield return new WaitForEndOfFrame();
        }

        float t = 0.0f;
        while (t < 1.0f)
        {
            m_text.color = Color.Lerp(Color.white, Color.clear, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
