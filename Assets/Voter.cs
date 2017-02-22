﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voter : Unit
{
    Cluster m_myCluster = null;

    [SerializeField]
    private LineRenderer m_lineRenderer;
    private float moverride_movementForce = 300.0f;

    protected float m_hitPoints = 1;

    [SerializeField]
    private List<Slot> m_slots;

    private Slot m_targetSlot;

    [SerializeField]
    private List<SpringJoint2D> m_initLineLeader;
    private Queue<SpringJoint2D> m_lineLeader;

    [SerializeField]
    private AudioClip m_positiveConversion;
    

    [SerializeField]
    private AudioClip m_negativeConversion;

    private const float MC_LEADER_MOVEMENT_MODIFIER = 1.75f;
    private const float MC_VOTER_MOVEMENT_MODIFIER = 1.0f;


    float m_minMovementInterval = .7f;
    float m_maxMovementInterval = 1.4f;


    void Awake()
    {
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
        //m_lineRenderer.numPositions = 2;
        //m_lineRenderer.SetPosition(0, transform.position);
        //m_lineRenderer.SetPosition(1, position.transform.position);

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

    public void DontGetWavedAt()
    {
        if (m_lineRenderer == null)
        {
            return;
        }
        m_lineRenderer.numPositions = 0;
    }

    public void TurnRed()
    {
        if (m_immortal)
        {
            return;
        }



        if (m_team == Team.BlueTeam)
        {
            m_audiosource.clip = (m_positiveConversion);
            //m_audiosource.Play();

            m_team = Team.RedTeam;
            m_animatorBlue.SetActive(false);
            m_animatorRed.SetActive(true);
            GameManager.ms_instance.GainConversionPoint(this);
        }
    }

    public Team GetTeam()
    {
        return m_team;
    }

    public void TurnBlue()
    {
     //   m_audiosource.PlayOneShot(m_negativeConversion);
        m_team = Team.BlueTeam;

        m_animatorBlue.SetActive(true);
        m_animatorRed.SetActive(false);
        GameManager.ms_instance.LoseConversionPoint(this);
    }

    public void CastRedVote(bool isLeader)
    {

        GameManager.ms_instance.PlayVoteSound();
        GameManager.ms_instance.VoteRed(this,isLeader);

        if (m_myCluster != null)
        {
            m_myCluster.RemoveMember(this);
        }

        Destroy(this.gameObject);
    }



    public void CastBlueVote(bool isLeader)
    {
        GameManager.ms_instance.VoteBlue(this,isLeader);



        if (m_myCluster != null)
        {
            m_myCluster.RemoveMember(this);
        }

        Destroy(this.gameObject);
    }

    private void FindMegaCluster()
    {
        m_myCluster = GameManager.ms_instance.GetNearestMegaCluster(this);
        if (m_myCluster == null)
        {
            m_myCluster = GameManager.ms_instance.CreateMegaCluster();
            m_myCluster.transform.position = this.transform.position;
        }
        m_myCluster.AddMember(this);
    }

    public void SetLineLeader(Queue<SpringJoint2D> springJoint)
    {
        m_lineLeader = springJoint;
    }

    private void MakeLineLeader(Voter voter)
    {
        Destroy(m_lineLeader.Dequeue());
        SpringJoint2D springJoint = voter.gameObject.AddComponent<SpringJoint2D>();

        springJoint.connectedBody = this.m_rigidbody;

        m_lineLeader.Enqueue(springJoint);

        voter.SetLineLeader(m_lineLeader);
        m_lineLeader = new Queue<SpringJoint2D>();
        m_lineLeader = null;
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
            if(this is Leader)
            {

                MoveTo(m_myCluster.transform.position, moverride_movementForce * 100.0f * MC_LEADER_MOVEMENT_MODIFIER);
            }
            MoveTo(m_myCluster.transform.position, moverride_movementForce * MC_VOTER_MOVEMENT_MODIFIER);
            return;
        }

        if (this is Leader)
        {

            MoveTo(m_myCluster.transform.position, moverride_movementForce * 100.0f * MC_LEADER_MOVEMENT_MODIFIER);
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

            if (coll.gameObject.GetComponent<Voter>() is Leader)
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerMovement.ms_instance.transform.position - transform.position, Vector2.Distance(transform.position, PlayerMovement.ms_instance.transform.position) + 1.0f, m_ignoreVotersMask);
            Debug.DrawRay(transform.position, PlayerMovement.ms_instance.transform.position - transform.position, Color.red, 1.0f);


            if (m_myCluster != null)
            {
                m_myCluster.RemoveMember(this);
                m_myCluster = null;
            }

            VotingBooth booth = GameManager.ms_instance.GetVotingBoothInRange(transform);

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

    private bool m_immortal = false;

    [SerializeField]
    private SpriteRenderer m_renderer;
    
    private IEnumerator WaitToImmortalize()
    {
        m_renderer.color = Color.black;

        m_immortal = true;
        yield return new WaitForSeconds(5.0f);
        float t = 0.0f;
        while(t < 1.0f)
        {
            m_renderer.color = Color.Lerp(Color.black, Color.white, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        m_renderer.color = Color.white;
        m_immortal = false;
    }

    protected IEnumerator VoterRoutine()
    {
        StartCoroutine(WaitToImmortalize());
        while (true)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerMovement.ms_instance.transform.position - transform.position, Vector2.Distance(transform.position, PlayerMovement.ms_instance.transform.position) + 1.0f, m_ignoreVotersMask);

            bool hasEnemies = GameManager.ms_instance.HasEnemiesNearby(this);



            //find out if there are enemies nearby, if there are go to clustering
            if (hasEnemies)
            {

                VotingBooth booth = GameManager.ms_instance.GetVotingBoothInRange(transform);
                if (booth != null)
                {
                    Debug.Log(booth);
                    if(this is Leader)
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

                    //Debug.Log("HasEnemies");
                    ClusterBehaviour();

                }

            }
            else
            {
                if (m_myCluster != null)
                {
                    m_myCluster.RemoveMember(this);
                    m_myCluster = null;
                }


                VotingBooth booth = GameManager.ms_instance.GetVotingBoothInRange(transform);

                if (booth != null)
                {
                    //Debug.Log("Going to the polls");
                    MoveTo(booth.transform.position, moverride_movementForce);

                }
                else if (Vector2.Distance(PlayerMovement.ms_instance.transform.position, this.transform.position) < Unit.ms_detectionRadius && this.GetTeam() == Team.RedTeam)
                {
                    //If there is no voting poll nearby look for the player. If he's there follow him.
                    if (this is Leader)
                    {

                        MoveTo(PlayerMovement.ms_instance.transform.position, moverride_movementForce * 100.0f);
                    }
                    else
                    {
                        MoveTo(PlayerMovement.ms_instance.transform.position, moverride_movementForce);
                    }

                }
                else
                {

                    ClusterBehaviour();
                }
            }



            yield return new WaitForSeconds(Random.Range(m_minMovementInterval, m_maxMovementInterval));
        }
    }

}
