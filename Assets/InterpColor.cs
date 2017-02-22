using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpColor : MonoBehaviour {

    [SerializeField]
    private SpriteRenderer m_spriteRenderer;

    [SerializeField]
    private Color m_color;

	// Use this for initialization
	void Start () {
        StartCoroutine(BlinkRoutine());
	}
	
    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                m_spriteRenderer.color = Color.Lerp(Color.white, m_color, t);

                yield return new WaitForEndOfFrame();
            }

            t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                m_spriteRenderer.color = Color.Lerp(m_color, Color.white, t);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
