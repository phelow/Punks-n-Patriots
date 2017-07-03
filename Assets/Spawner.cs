using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> mp_voterPrefabs;

    private int m_maxEnemiesIterator = 0;

    [SerializeField]
    private List<int> m_maxEnemiesOverTime;

    [SerializeField]
    private int m_maxEnemies = 20;

    [SerializeField]
    private List<Voter> m_myVoters;

    // Use this for initialization
    void Start()
    {
        m_myVoters = new List<Voter>();
        SetMaxEnemiesNext();
        StartCoroutine(SpawnEnemies());
    }

    public void SetMaxEnemiesNext()
    {
        m_maxEnemies = m_maxEnemiesOverTime[m_maxEnemiesIterator];
        m_maxEnemiesIterator++;
    }


    private IEnumerator SpawnEnemies()
    {
        // for the first thirty seconds only spawn regular protestors
        float time = 30.0f;

        while(m_maxEnemies <= 0)
        {
            yield return new WaitForSeconds(1.0f);
            time += 1.0f;
        }

        while(time > 0.0f)
        {
            Voter voter = GameObject.Instantiate(mp_voterPrefabs[0], transform.position, transform.rotation, null).GetComponent<Voter>();
            m_myVoters.Add(voter);
            GameManager.ms_instance.AddVoter(voter);
            float deltaTime = Random.Range(.8f, 3.5f);
            time -= deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }

        while (true)
        {
            m_myVoters.RemoveAll(item => item == null);
            m_myVoters.RemoveAll(v => v.GetTeam() == Unit.Team.RedTeam);
            
            if (m_myVoters.Count > m_maxEnemies)
            {

                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                Voter voter = GameObject.Instantiate(mp_voterPrefabs[Random.Range(0, mp_voterPrefabs.Count)], transform.position, transform.rotation, null).GetComponent<Voter>();
                m_myVoters.Add(voter);
                GameManager.ms_instance.AddVoter(voter);

                yield return new WaitForSeconds(Random.Range(.8f, 3.5f));
            }
        }
    }

}
