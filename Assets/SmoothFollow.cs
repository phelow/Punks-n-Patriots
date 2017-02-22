using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {
    [SerializeField]
    private GameObject m_player;

    [SerializeField]
    private Rigidbody2D m_rigidbody;
	
	// Update is called once per frame
	void Update () {
        m_rigidbody.AddForce((m_player.transform.position - transform.position).normalized * (1 + 10 *Vector2.Distance(m_player.transform.position,transform.position)) * 10000.0f * Time.deltaTime);
	}
}
