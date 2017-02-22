using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaCluster : Cluster {

    [SerializeField]
    private Rigidbody2D m_rigidbody;
    private float mc_fleeForce = 50.0f;

    

    public void StartMegaCluster()
    {
        m_targetSize = UnityEngine.Random.Range(12, 24);
        m_members = new List<Voter>();

        StartCoroutine(EvadePlayer());
    }

    private IEnumerator EvadePlayer()
    {
        while (m_members.Count < 3)
        {
            m_rigidbody.AddForce((transform.position - PlayerMovement.ms_instance.transform.position).normalized * mc_fleeForce);
            yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));
        }
    }
}
