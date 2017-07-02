using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutOnRightClick : MonoBehaviour
{
    [SerializeField]
    private Text m_text;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(FadeOnClick());
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

        while (!Input.GetMouseButton(1))
        {
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
