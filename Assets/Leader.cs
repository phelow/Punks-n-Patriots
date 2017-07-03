using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Voter
{

    private const float MC_PROTESTOR_KICK_RATE = 500.0f;
    [SerializeField]
    private LayerMask m_showEverything;


    // Use this for initialization
    void Start()
    {
        StartCoroutine(LeaderRoutine());
    }

    public bool IsCharging()
    {
        return m_rigidbody.velocity.magnitude > .01f;
    }

    /// <summary>
    /// Leaders are highly resistant to being converted by collision and it's almost impossible for this to happen
    /// </summary>
    /// <param name="coll"></param>
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Voter")
        {
            if (coll.gameObject.GetComponent<Voter>().GetTeam() == GetTeam())
            {
                m_hitPoints += 10;
            }
            else
            {
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
        if (IsCharging() && CanKick(coll))
        {
            KickAwayFromLeader(coll.gameObject);
        }
    }

    public bool CanKick(Collision2D coll)
    {
        if(coll.gameObject == null)
        {
            return false;
        }

        Voter voter = coll.gameObject.GetComponent<Voter>();
        if (voter != null && voter.GetTeam() != GetTeam())
        {
            return true;
        }

        PlayerMovement player = coll.gameObject.GetComponentInChildren<PlayerMovement>();

        if(player != null && GetTeam() == Team.BlueTeam)
        {
            return true;
        }

        return false;
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (IsCharging() && CanKick(coll))
        {
            KickAwayFromLeader(coll.gameObject);
        }

    }


    private void KickAwayFromLeader(GameObject go)
    {
        Rigidbody2D rb = go.GetComponentInChildren<Rigidbody2D>();

        if(rb == null)
        {
            return;
        }

        rb.AddForce((go.transform.position - transform.position).normalized * MC_PROTESTOR_KICK_RATE * rb.mass);
    }


    public override void TurnRed()
    {
        if (m_immortal)
        {
            return;
        }



        if (m_team == Team.BlueTeam)
        {
            m_audiosource.clip = (m_positiveConversion);
            //m_audiosource.Play();

            m_hitPoints = 30;
            m_team = Team.RedTeam;
            m_animatorBlue.SetActive(false);
            m_animatorRed.SetActive(true);
            GameManager.ms_instance.GainConversionPoint(this);
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
