using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Voter {
    [SerializeField]
    private LayerMask m_showEverything;
    
    // Use this for initialization
    void Start()
    {
        StartCoroutine(LeaderRoutine());
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Voter")
        {
            if (coll.gameObject.GetComponent<Voter>() is Leader)
            {
                m_hitPoints = 0;
            }

            if (coll.gameObject.GetComponent<Voter>().GetTeam() == GetTeam())
            {
                m_hitPoints += 3;
            }
            else
            {
                m_hitPoints /= 2;
                m_hitPoints--;
            }

            if (m_hitPoints <= 0)
            {
                m_hitPoints = 10;
                if (GetTeam() == Team.BlueTeam)
                {
                    TurnRed();
                }
                else
                {
                    TurnBlue();
                }
            }

        }
    }
    private IEnumerator LeaderRoutine()
    {
        yield return VoterRoutine();
        /*
        while (true)
        {
            if(GetTeam() == Team.RedTeam)
            {
                yield return PassiveLeaderRoutine();
            }
            else
            {
                //pick nearest enemy
                Voter nextEnemy = null;
                while (nextEnemy == null)
                {
                    RaycastHit2D hit = Physics2D.Raycast(this.transform.position, PlayerMovement.ms_instance.transform.position - transform.position, Vector2.Distance(this.transform.position, PlayerMovement.ms_instance.transform.position), m_ignoreVotersMask);

                    if (hit != null && hit.collider != null && hit.collider.gameObject != null && hit.collider.gameObject == PlayerMovement.ms_instance.gameObject == PlayerMovement.ms_instance.gameObject)
                    {
                        MoveTo(PlayerMovement.ms_instance.transform.position,100000.0f);

                    }
                    nextEnemy = GameManager.ms_instance.GetNearestEnemy(this);
                    yield return new WaitForSeconds(Random.Range(0.3f, .7f));
                }

                while (nextEnemy != null && nextEnemy.GetTeam() != GetTeam() && Physics2D.Raycast(transform.position, nextEnemy.transform.position - transform.position, Vector2.Distance(transform.position, nextEnemy.transform.position), m_showEverything))
                {

                    if (nextEnemy == null)
                    {

                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        //move towards enemy until
                        MoveTo(nextEnemy.transform.position);
                    }

                    //A: they move out of range
                    //B: They are converted
                    yield return new WaitForSeconds(Random.Range(0.3f, .7f));
                }

            }

        }*/
    }
}
