using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voter : Unit
{
    Cluster m_myCluster = null;

    private const float MC_RED_LEADER_VOTING_BOOTH_DISTANCE = 10000.0F;
    private const float MC_NORMAL_VOTING_BOOTH_DISTANCE = 3.0f;

    [SerializeField]
    private LineRenderer m_lineRenderer;
    private float moverride_movementForce = 300.0f;

    protected float m_hitPoints = 1;

    [SerializeField]
    private List<Slot> m_slots;

    [SerializeField]
    private bool m_startNow = false;

    private Slot m_targetSlot;

    [SerializeField]
    private List<SpringJoint2D> m_initLineLeader;
    private Queue<SpringJoint2D> m_lineLeader;

    [SerializeField]
    protected AudioClip m_positiveConversion;


    [SerializeField]
    private AudioClip m_negativeConversion;

    private const float MC_LEADER_MOVEMENT_MODIFIER = 1.75f;
    private const float MC_VOTER_MOVEMENT_MODIFIER = 1.0f;


    float m_minMovementInterval = .7f;
    float m_maxMovementInterval = 1.4f;

    public static int ms_numCharacters = 0;


    void Awake()
    {
        ms_numCharacters++;
        foreach (SpringJoint2D joint in m_initLineLeader)
        {
            m_lineLeader.Enqueue(joint);
        }
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(VoterRoutine());
    }


    public virtual bool IsLeader()
    {
        return false;
    }

    public List<Slot> GetSlots()
    {
        return m_slots;
    }

    public void KickFromCluster()
    {
        m_myCluster = null;
    }

    public void GetWavedAt(Transform position)
    {
        if (GetTeam() == Team.BlueTeam)
        {
            return;
        }

        Vector2 vacuumForce = (new Vector2(position.position.x, position.position.y) - new Vector2(transform.position.x, transform.position.y)).normalized * Time.deltaTime * 100.0f * Mathf.Pow(Vector2.Distance(position.position, transform.position), 2);

        m_rigidbody.AddForce(vacuumForce);
    }

    public void ProcessWave()
    {
        TurnRed();
    }

    public virtual void TurnRed()
    {
        if (this.GetTeam() == Team.RedTeam)
        {
            return;
        }

        if (m_immortal)
        {
            return;
        }

        if (m_team == Team.BlueTeam)
        {
            m_hitPoints = 20;
            m_audiosource.clip = (m_positiveConversion);
            //m_audiosource.Play();

            m_team = Team.RedTeam;
            m_animatorBlue.gameObject.SetActive(false);
            m_animatorRed.gameObject.SetActive(true);
            GameManager.ms_instance.GainConversionPoint(this);
        }
    }

    public Team GetTeam()
    {
        return m_team;
    }

    public float GetHealth()
    {
        return m_immortal ? 100.0f : m_hitPoints;
    }

    public void TurnBlue()
    {
        if (this.GetTeam() == Team.BlueTeam)
        {
            return;
        }

        //   m_audiosource.PlayOneShot(m_negativeConversion);
        m_team = Team.BlueTeam;

        m_animatorBlue.gameObject.SetActive(true);
        m_animatorRed.gameObject.SetActive(false);
        GameManager.ms_instance.LoseConversionPoint(this);
    }

    public void CastRedVote(bool isLeader)
    {

        GameManager.ms_instance.PlayVoteSound();
        GameManager.ms_instance.VoteRed(this, isLeader);

        if (m_myCluster != null)
        {
            m_myCluster.RemoveMember(this);
        }

        ms_numCharacters--;

        Destroy(this.gameObject);
    }



    public void CastBlueVote(bool isLeader)
    {
        GameManager.ms_instance.VoteBlue(this, isLeader);



        if (m_myCluster != null)
        {
            m_myCluster.RemoveMember(this);
        }
        ms_numCharacters--;
        Destroy(this.gameObject);
    }

    protected void ClusterBehaviour()
    {
        if (m_myCluster == null)
        {
            //find nearest cluster in view
            m_myCluster = GameManager.ms_instance.GetNearestCluster(this);

            if (m_myCluster == null)
            {
                m_myCluster = GameManager.ms_instance.CreateCluster();
                m_myCluster.transform.position = this.transform.position;
            }


        }
        if (m_targetSlot == null || m_targetSlot.transform == null)
        {
            m_targetSlot = m_myCluster.AddMember(this);
        }

        if (m_targetSlot == null)
        {
            MoveTo(m_myCluster.transform.position, moverride_movementForce * MC_VOTER_MOVEMENT_MODIFIER);
            return;
        }

        MoveTo(m_targetSlot.transform.position, moverride_movementForce * MC_VOTER_MOVEMENT_MODIFIER);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Voter")
        {
            if (m_immortal)
            {
                m_hitPoints = 10;
            }

            if (coll.gameObject.GetComponent<Voter>() is Leader && coll.gameObject.GetComponent<Voter>().GetTeam() != this.GetTeam())
            {
                m_hitPoints = 0;
            }

            if (coll.gameObject.GetComponent<Voter>().GetTeam() == GetTeam())
            {
                m_hitPoints++;
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

    protected IEnumerator PassiveLeaderRoutine()
    {
        while (true)
        {
            if (m_myCluster != null)
            {
                m_myCluster.RemoveMember(this);
                m_myCluster = null;
            }

            VotingBooth booth = GameManager.ms_instance.GetVotingBoothInRange(transform, this.GetTeam() == Team.RedTeam ? MC_RED_LEADER_VOTING_BOOTH_DISTANCE : MC_NORMAL_VOTING_BOOTH_DISTANCE);

            if (booth != null)
            {

                MoveTo(booth.transform.position, moverride_movementForce);

            }
            else if (Vector2.Distance(PlayerMovement.ms_instance.transform.position, this.transform.position) < Unit.ms_detectionRadius)
            {
                //If there is no voting poll nearby look for the player. If he's there follow him.
                MoveTo(PlayerMovement.ms_instance.transform.position, moverride_movementForce);

            }

            yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval));
        }
    }

    protected bool m_immortal = false;

    [SerializeField]
    private SpriteRenderer m_renderer;

    private IEnumerator WaitToImmortalize()
    {

        if (!m_startNow)
        {
            m_renderer.color = Color.black;

            m_immortal = true;
            yield return new WaitForSeconds(5.0f);
            float t = 0.0f;
            while (t < 1.0f)
            {
                m_renderer.color = Color.Lerp(Color.black, Color.white, t);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        m_renderer.color = Color.white;
        m_immortal = false;
    }

    protected IEnumerator VoterRoutine()
    {
        StartCoroutine(WaitToImmortalize());
        int babySteps = 10;
        while (true)
        {
            if (m_hitPoints > 20)
            {
                m_hitPoints -= 5;
            }

            bool sprintToPolls = GameManager.GetTimeLeft() < 30;

            VotingBooth booth = GameManager.ms_instance.GetVotingBoothInRange(transform, sprintToPolls ? Mathf.Infinity : (this.GetTeam() == Team.RedTeam ? 2 : 1) * MC_NORMAL_VOTING_BOOTH_DISTANCE * (this is Leader ? 100 : 1));
            Voter hasEnemies = GameManager.ms_instance.HasEnemiesNearby(this);

            if (sprintToPolls && booth != null)
            {
                MoveTo(booth.transform.position, moverride_movementForce * MC_VOTER_MOVEMENT_MODIFIER);

                if (this is Leader)
                {
                    yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval) / 2);
                }
                else
                {
                    yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval));
                }

                continue;
            }

            if (booth != null && ((this is Leader && this.GetTeam() == Team.RedTeam) || hasEnemies == null))
            {
                if (this is Leader)
                {
                    MoveTo(booth.transform.position, moverride_movementForce * 100.0f * MC_LEADER_MOVEMENT_MODIFIER);
                }
                else
                {
                    MoveTo(booth.transform.position, moverride_movementForce * MC_VOTER_MOVEMENT_MODIFIER);
                }
            }
            else
            {
                ClusterBehaviour();
            }


            Vector3 viewPosition = Camera.main.WorldToViewportPoint(this.transform.position);

            if (babySteps-- < 0 && (viewPosition.x > 1.0f || viewPosition.x < 0.0f || viewPosition.y < 0.0f || viewPosition.y > 1.0f))
            {
                yield return new WaitForSeconds(3.0f);
            }
            else
            {
                if (this is Leader)
                {

                    yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval) / 2);
                }
                else
                {
                    yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval));

                }

            }
        }
    }

}
