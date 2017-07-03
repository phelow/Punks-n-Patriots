using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingText : MonoBehaviour {
    [SerializeField]
    private TextMesh m_textMesh;

    [SerializeField]
    private Rigidbody2D m_rigidbody;
	// Use this for initialization
	void Start () {
        StartCoroutine(TextDissapear());
	}

    private IEnumerator TextDissapear()
    {
        float tPassed = 0.0f;
        m_rigidbody.AddForce(Vector2.up * 10.0f);
        while (tPassed < 20.0f)
        {
            tPassed += Time.deltaTime;

            m_textMesh.color = Color.Lerp(m_textMesh.color, Color.clear, tPassed/ 20.0f);

            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
