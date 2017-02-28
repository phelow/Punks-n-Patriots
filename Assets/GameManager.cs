using System.Collections;
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
    private int m_currentPoints = 15;

    [SerializeField]
    public LayerMask m_ignoreVoters;
    [SerializeField]
    private LayerMask m_ignoreClusters;
    [SerializeField]
    private int m_timeLeft = 480;

    [SerializeField]
    private Text m_votesText;
    [SerializeField]
    private Text m_pointsNeededText;

    [SerializeField]
    private Text m_timeText;


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
    private int m_pointsNeeded = 30;

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

    public void RemoveVoter(Voter voter)
    {
        m_voters.Remove(voter);
    }

    public void AddVotingBooth(VotingBooth booth)
    {
        if(m_booths == null)
        {
            m_booths = new List<VotingBooth>();
        }

        m_booths.Add(booth);
    }

    public void AddVoter(Voter voter)
    {
        m_voters.Add(voter);
    }

    public VotingBooth GetVotingBoothInRange(Transform startingPoint, float range)
    {
        VotingBooth closest = null;
        if(m_booths == null)
        {
            Debug.Log("nullbooths");
            return null;
        }

        foreach(VotingBooth booth in m_booths)
        {
            float dist = Vector2.Distance(booth.transform.position, startingPoint.position);

            if (dist < range)
            {
                RaycastHit2D hit = Physics2D.Raycast(startingPoint.position, booth.transform.position - startingPoint.position, dist, GameManager.ms_instance.m_ignoreVoters);

                Debug.DrawRay(startingPoint.position, booth.transform.position - startingPoint.position, Color.blue, 1.0f);

                if(hit.collider != null)
                {
                    if (hit.collider.gameObject != null)
                    {
                        if (hit.collider.gameObject.tag == "PollingStation")
                        {
                            if(closest == null)
                            {
                                closest = booth;
                            }

                            if(Vector2.Distance(booth.transform.position,startingPoint.position) < Vector2.Distance(closest.transform.position, startingPoint.position))
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
    void Awake() {
        AudioListener.volume = 10.0f;

        ms_instance = this;
        m_voters = GameObject.FindGameObjectsWithTag("Voter").Select(x => x.GetComponent<Voter>()).ToList();

        m_clusters = new List<Cluster>();
        m_currentPoints--;
        m_pointsNeededText.text = "Approval: " + m_currentPoints + "%" +  " Goal: " + m_pointsNeeded + "%";



        StartCoroutine(TimeLevel());
    }

    public Voter GetNearestEnemy(Voter voter)
    {

        foreach (Voter person in m_voters)
        {
            if (person.GetTeam() != voter.GetTeam())
            {
                RaycastHit2D hit = Physics2D.Raycast(voter.transform.position, person.transform.position - voter.transform.position, Vector2.Distance(person.transform.position, voter.transform.position) + 1.0f,m_ignoreClusters);

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

    private IEnumerator TimeLevel()
    {
        for (int i = m_timeLeft; i > 0; i--)
        {
            m_timeText.text = "" + i;


            yield return new WaitForSeconds(1.0f);
        }

        //TODO: time warning effects
        EndGame();


    }

    public void VoteBlue(Voter voter, bool isLeader)
    {
        int pointsToLose = isLeader == false ? 2 : 5;
        
        string pointsToLoseText = "-" + pointsToLose;


        GainPoints(-pointsToLose, pointsToLoseText, Color.red);
    }

    public void PlayVoteSound()
    {
        m_effectsSource.clip =(m_voteClip);
        m_effectsSource.Play();
    }

    public void GainConversionPoint(Voter voter)
    {
        string pointsToGainText = "+" + MC_CONVERSION_POINTS;
        GainPoints(MC_CONVERSION_POINTS, pointsToGainText, Color.green);
    }


    public void LoseConversionPoint(Voter voter)
    {
        string pointsToGainText = "-" + MC_CONVERSION_POINTS;
        GainPoints(-MC_CONVERSION_POINTS, pointsToGainText, Color.red);
    }

    public void VoteRed(Voter voter, bool isLeader)
    {
        int pointsToGain = isLeader == false ? 2 : 5;

        string pointsToGainText = "+" + pointsToGain;
        GainPoints(pointsToGain,pointsToGainText, Color.green);
    }

    public void GainPoints(int pointsToGain,string pointsToGainText, Color textColor)
    {
        m_currentPoints += pointsToGain;
        m_pointsNeededText.text = "Approval: " + m_currentPoints + "%" + "    Goal: " + m_pointsNeeded + "%";


        Text t = GameObject.Instantiate(mp_fallingText, m_fallingTextSpawnPoint.transform.position + new Vector3(Random.Range(20.0f, 50.0f), Random.Range(-20.0f, 20.0f), 0.0f), transform.rotation, transform).GetComponent<Text>();
        t.color = textColor;

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

        if(PlayerPrefs.GetInt("HighScore",0) < m_currentPoints)
        {
            PlayerPrefs.SetInt("HighScore", m_currentPoints);
        }

        PlayerPrefs.SetInt("YourScore", m_currentPoints);

        if (m_currentPoints > m_pointsNeeded)
        {
            //TODO: win effects
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
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

    public Cluster CreateMegaCluster()
    { //TODO: cleanup, make subfunctions
        MegaCluster cluster = GameObject.Instantiate(mp_megaCluster, null).GetComponent<MegaCluster>();
        cluster.StartMegaCluster();



        StartCoroutine(cluster.TickDown());

        m_clusters.Add(cluster);

        return cluster;

    }

    public Cluster CreateCluster()
    {
        Cluster cluster = GameObject.Instantiate(mp_cluster, null).GetComponent<Cluster>();
        cluster.StartCluster();



        StartCoroutine(cluster.TickDown());

        m_clusters.Add(cluster);

        return cluster;
    }

    public bool HasEnemiesNearby(Voter voter)
    {
        foreach (Voter person in m_voters)
        {
            if (person.GetTeam() != voter.GetTeam())
            {
                RaycastHit2D hit = Physics2D.Raycast(voter.transform.position, person.transform.position- voter.transform.position, Vector2.Distance(person.transform.position,voter.transform.position) + 1.0f);

                if (hit.collider != null)
                {

                    Voter collidedVoter = hit.collider.gameObject.GetComponent<Voter>();
                    if (collidedVoter != null && collidedVoter.GetTeam() != voter.GetTeam())
                    {

                        return true;
                    }
                }
            }
        }

        return false;
    }

    public Cluster CheckForClusters(List<Cluster> clusters, Voter voter)
    {
        foreach (Cluster cluster in clusters)
        {
            Vector3 center = cluster.GetCenter();

            if (Physics2D.RaycastAll(center, voter.transform.position - center, Vector2.Distance(center, transform.position)).Where(x => x == voter).ToList().Count > 0 && cluster.HasRoom())
            {
                return cluster;
            }
        }
        return null;
    }

    public Cluster GetNearestMegaCluster(Voter voter)
    {
        return CheckForClusters(m_clusters.Where(x => x is MegaCluster).ToList(), voter);
        

    }

    public Cluster GetNearestCluster(Voter voter)
    {

        return CheckForClusters(Physics2D.OverlapCircleAll(voter.transform.position,4.0f,m_onlyClusters).Select(x=> x.GetComponent<Cluster>()).ToList(), voter);
    }

    public void RemoveCluster(Cluster cluster)
    {
        ms_instance.m_clusters.Remove(cluster);
        Destroy(cluster.gameObject);
    }

    // Update is called once per frame
    void Update () {
        m_sourceOne.volume = 1.0f;
        
        if(m_currentPoints > 25)
        {
            m_sourceTwo.volume += Time.deltaTime;
            
        }

        if(m_currentPoints > 50)
        {
            m_sourceThree.volume += Time.deltaTime;
        }

    }
}
