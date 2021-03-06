﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int MC_CONVERSION_POINTS = 1;

    public static GameManager ms_instance;
    public List<Cluster> m_clusters;
    public List<Voter> m_voters;
    private int m_currentPoints = 0;

    [SerializeField]
    private List<Spawner> m_spawners;

    [SerializeField]
    private List<int> m_timeIntervals;

    [SerializeField]
    public LayerMask m_ignoreVoters;
    [SerializeField]
    private LayerMask m_ignoreClusters;
    [SerializeField]
    private int m_timeLeft = 360;

    [SerializeField]
    private Text m_votesText;
    [SerializeField]
    private Text m_pointsNeededText;

    [SerializeField]
    private Text m_timeText;

    // what percentage of voters spawn on your side
    private int m_spawnRatio = 0;

    [SerializeField]
    private AudioClip m_voteClip;

    [SerializeField]
    private AudioSource m_audioSource;

    public GameObject mp_cluster;
    public GameObject mp_megaCluster;

    private List<VotingBooth> m_booths = null;
    [SerializeField]
    private LayerMask m_onlyClusters;
    
    [SerializeField]
    private int m_pointsNeeded = 200;

    [SerializeField]
    private GameObject mp_fallingText;
    [SerializeField]
    private GameObject m_fallingTextSpawnPoint;

    [SerializeField]
    private AudioSource m_sourceOne;
    [SerializeField]
    private AudioSource m_sourceTwo;
    [SerializeField]
    private AudioSource m_sourceThree;
    [SerializeField]
    private AudioSource m_effectsSource;

    private const float c_finalRushStart = 60.0f;

    private const float c_megaFinalRushStart = 30.0f;

    public void RemoveVoter(Voter voter)
    {
        m_voters.Remove(voter);
    }

    public void AddVotingBooth(VotingBooth booth)
    {
        if (m_booths == null)
        {
            m_booths = new List<VotingBooth>();
        }

        m_booths.Add(booth);
    }

    public static int SpawnRatio
    {
        get
        {
            return ms_instance.m_spawnRatio;
        }
    }

    public void AddVoter(Voter voter)
    {
        m_voters.Add(voter);
    }

    public VotingBooth GetVotingBoothInRange(Transform startingPoint, float range)
    {
        VotingBooth closest = null;
        if (m_booths == null)
        {
            Debug.Log("nullbooths");
            return null;
        }

        foreach (VotingBooth booth in m_booths)
        {
            float dist = Vector2.Distance(booth.transform.position, startingPoint.position);

            if (dist < range)
            {
                RaycastHit2D hit = Physics2D.Raycast(startingPoint.position, booth.transform.position - startingPoint.position, dist, GameManager.ms_instance.m_ignoreVoters);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject != null)
                    {
                        if (hit.collider.gameObject.tag == "PollingStation")
                        {
                            if (closest == null)
                            {
                                closest = booth;
                            }

                            if (Vector2.Distance(booth.transform.position, startingPoint.position) < Vector2.Distance(closest.transform.position, startingPoint.position))
                            {
                                closest = booth;

                            }
                        }
                        else
                        {
                            //Debug.Log( "miss:" + hit.collider.gameObject);
                        }
                    }

                }
            }
        }


        return closest;
    }


    // Use this for initialization
    void Awake()
    {
        AudioListener.volume = 10.0f;

        ms_instance = this;
        m_voters = GameObject.FindGameObjectsWithTag("Voter").Select(x => x.GetComponent<Voter>()).ToList();

        m_clusters = new List<Cluster>();
        m_pointsNeededText.text = "SUPPORTERS: " + m_currentPoints + "  GOAL: " + m_pointsNeeded;



        StartCoroutine(TimeLevel());
        StartCoroutine(BlinkText());
    }

    private IEnumerator BlinkText()
    {
        yield return new WaitForEndOfFrame();
        while (!GameManager.IsFinalRush())
        {
            yield return new WaitForSeconds(1.0f);
        }

        while (true)
        {
            yield return new WaitForSeconds(.5f);
            m_timeText.color = Color.black;
            yield return new WaitForSeconds(.5f);
            m_timeText.color = Color.clear;
        }
    }

    public static int GetTimeLeft()
    {
        return ms_instance.timeLeft;
    }

    public static bool IsFinalRush()
    {
        return ms_instance.timeLeft <= c_finalRushStart;
    }

    public static bool IsMegaFinalRush()
    {
        return ms_instance.timeLeft <= c_megaFinalRushStart;
    }

    private int timeLeft = 9999;
    private IEnumerator TimeLevel()
    {
        yield return new WaitForEndOfFrame();
        for (timeLeft = m_timeLeft; timeLeft > 0; timeLeft--)
        {
            int minutes = (int)(timeLeft / 60.0f);

            m_timeText.text = "" + minutes + (":" + ("" + (timeLeft - (minutes * 60))).PadLeft(2, '0')) + " UNTIL POLLS CLOSE";

            if (timeLeft == 60)
            {
                m_spawnRatio = m_currentPoints;
            }

            if(timeLeft == 120)
            {
                foreach(Spawner spawner in m_spawners)
                {
                    spawner.TriggerTwoMinuteSpawn();
                }
            }
            else if (timeLeft == 60)
            {

                foreach (Spawner spawner in m_spawners)
                {
                    spawner.TriggerOneMinuteSpawn();
                }
            }
            else if (timeLeft == 15)
            {

                foreach (Spawner spawner in m_spawners)
                {
                    spawner.TriggerFinalRush();
                }
            }

            if (m_timeIntervals.Contains(timeLeft))
            {

                //Debug.Log(timeLeft);
                SetMaxEnemiesOnSpawners();
            }

            yield return new WaitForSeconds(1.0f);
        }

        EndGame();
    }

    public void SetMaxEnemiesOnSpawners()
    {
        foreach (Spawner spawner in m_spawners)
        {
            spawner.SetMaxEnemiesNext();
        }
    }

    public void VoteBlue(Voter voter, bool isLeader)
    {
        if (!isLeader)
        {
            return;
        }

        int pointsToLose = isLeader == false ? 2 : 5;

        string pointsToLoseText = "-" + pointsToLose;


        GainPoints(-pointsToLose, pointsToLoseText, Color.red, voter.transform.position);
    }

    public void PlayVoteSound()
    {
        m_effectsSource.clip = (m_voteClip);
        m_effectsSource.Play();
        m_effectsSource.time = .5f;
    }

    public void GainConversionPoint(Voter voter)
    {
        string pointsToGainText = "+" + MC_CONVERSION_POINTS;
        GainPoints(MC_CONVERSION_POINTS, pointsToGainText, Color.green, voter.transform.position);
    }


    public void LoseConversionPoint(Voter voter)
    {
        string pointsToGainText = "-" + MC_CONVERSION_POINTS;
        GainPoints(-MC_CONVERSION_POINTS, pointsToGainText, Color.red, voter.transform.position);
    }

    public void VoteRed(Voter voter, bool isLeader)
    {
        int pointsToGain = isLeader == false ? 1 : 5;

        string pointsToGainText = "+" + pointsToGain;
        GainPoints(pointsToGain, pointsToGainText, Color.green, voter.transform.position);
    }

    public void GainPoints(int pointsToGain, string pointsToGainText, Color textColor, Vector3 position)
    {
        m_currentPoints +=  pointsToGain;
        if (m_currentPoints < 0)
        {
            m_currentPoints = 0;
        }


        m_pointsNeededText.text = "SUPPORTERS: " + m_currentPoints + "  GOAL: " + m_pointsNeeded;


        GameObject t = GameObject.Instantiate(mp_fallingText, position, transform.rotation, transform);

        if(Mathf.Abs(pointsToGain) != 5)
        {
            t.transform.localScale *= 0.5f;
        }

        t.GetComponentInChildren<TextMesh>().color = textColor;
        t.GetComponentInChildren<TextMesh>().text = "" + (pointsToGain > 0 ? "+" : "") + pointsToGain;

        t.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-20.0f, 20.0f));

        StartCoroutine(DelayedDestroy(t.gameObject));
    }

    private IEnumerator DelayedDestroy(GameObject go)
    {
        yield return new WaitForSeconds(4.0f);
        Destroy(go);
    }


    public void EndGame()
    {
        PlayerPrefs.SetInt("YourScore", m_currentPoints);

        if (m_currentPoints >= m_pointsNeeded)
        {
            PlayerPrefs.SetString("" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1, "Unlocked");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Win");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Fail");
        }
    }

    public List<Voter> GetAllVoters()
    {

        return m_voters;
    }

    public Cluster CreateCluster()
    {
        Cluster cluster = GameObject.Instantiate(mp_cluster, null).GetComponent<Cluster>();
        cluster.StartCluster();

        StartCoroutine(cluster.TickDown());

        m_clusters.Add(cluster);

        return cluster;
    }

    public Voter HasEnemiesNearby(Voter voter)
    {
        Collider2D[] collisions;
        collisions = Physics2D.OverlapCircleAll(voter.transform.position, voter.IsLeader() ? 10.0f : 6.0f);
        foreach (Collider2D coll in collisions)
        {
            Voter v = coll.GetComponent<Voter>();
            if (v == null)
            {
                continue;
            }

            if (v.GetTeam() != voter.GetTeam())
            {
                RaycastHit2D hit = Physics2D.Raycast(voter.transform.position, v.transform.position - voter.transform.position, Vector2.Distance(v.transform.position, voter.transform.position) + 1.0f, m_ignoreClusters);

                if (hit.collider != null)
                {

                    Voter collidedVoter = hit.collider.gameObject.GetComponent<Voter>();
                    if (collidedVoter != null && collidedVoter.GetTeam() != voter.GetTeam())
                    {

                        return hit.collider.gameObject.GetComponent<Voter>();
                    }
                }
            }
        }

        return null;
    }

    public Cluster CheckForClusters(List<Cluster> clusters, Voter voter)
    {
        if (clusters.Count == 0)
        {
            return null;
        }

        Cluster cluster = clusters[Random.Range(0, clusters.Count)];

        return cluster;
    }

    public Cluster GetNearestCluster(Voter voter)
    {
        VotingBooth nearestBooth = GetVotingBoothInRange(voter.transform, 200.0f);

        Collider2D [] colliders = Physics2D.OverlapCircleAll(voter.transform.position, 10.0f, m_onlyClusters);
        Collider2D closestCollider = colliders.FirstOrDefault();

        if (nearestBooth == null)
        {
            return closestCollider != null ? closestCollider.GetComponent<Cluster>() : null;
        }

        foreach (Collider2D collider in colliders)
        {
            if (Vector2.Distance(collider.transform.position, nearestBooth.transform.position) < Vector2.Distance(closestCollider.transform.position, nearestBooth.transform.position))
            {
                closestCollider = collider;
            }
        }


        return closestCollider != null ? closestCollider.GetComponent<Cluster>() : null;
    }

    public void RemoveCluster(Cluster cluster)
    {
        ms_instance.m_clusters.Remove(cluster);
        Destroy(cluster.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        m_sourceOne.volume = 1.0f;

        if (m_currentPoints > 25)
        {
            m_sourceTwo.volume += Time.deltaTime;

        }

        if (m_currentPoints > 50)
        {
            m_sourceThree.volume += Time.deltaTime;
        }

    }
}
