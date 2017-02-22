using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour {
    [SerializeField]
    private List<GameObject> mp_voterPrefabs;

    [SerializeField]
    private int m_maxEnemies = 20;

	// Use this for initialization
	void Start () {
        StartCoroutine(SpawnEnemies());	
	}

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (GameManager.ms_instance.GetAllVoters().Where(x=>x.GetTeam() == Voter.Team.BlueTeam).ToList().Count < m_maxEnemies)
            {
                Voter voter = GameObject.Instantiate(mp_voterPrefabs[Random.Range(0, mp_voterPrefabs.Count)], transform.position, transform.rotation, null).GetComponent<Voter>();
                
                GameManager.ms_instance.AddVoter(voter);
            }
            yield return new WaitForSeconds(Random.Range(.8f, 3.5f));
        }
    }

}
