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
        StartCoroutine(VoterRoutine());
    }

    public override bool IsLeader()
    {
        return true;
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
            Voter voter = coll.gameObject.GetComponent<Voter>();
            if (voter.GetTeam() == GetTeam())
            {
                m_hitPoints += 10;
            }
            else
            {
                if (!voter.IsLeader())
                {
                    voter.LeaderConvert();
                }
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
            m_hitPoints = 30;
            m_team = Team.RedTeam;
            m_animatorBlue.gameObject.SetActive(false);
            m_animatorRed.gameObject.SetActive(true);
            GameManager.ms_instance.GainConversionPoint(this);
        }
    }
}
