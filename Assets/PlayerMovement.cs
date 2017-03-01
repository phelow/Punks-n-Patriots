using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovement : Unit
{
    [SerializeField]
    private GameObject m_waveRadius;

    private float m_defaultWaveRadius = .1f;

    [SerializeField]
    private LayerMask m_ignorePlayer;

    [SerializeField]
    private float scaleFactor = 2.6f;

    private float mc_originalScaleFactor = 3.0f;

    bool interrupted = false;
    public static PlayerMovement ms_instance;

    [SerializeField]
    private Animator m_animator;

    private const float MC_PROTESTOR_KICK_RATE = 100000.0f;

    [SerializeField]
    private AudioClip m_chargeClip;

    [SerializeField]
    private AudioClip m_deployClip;

    void Awake()
    {
        PlayerPrefs.SetString("LastLevel", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        ms_instance = this;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(MovePlayer());
    }

    private IEnumerator MovePlayer()
    {
        bool waving = false;
        while (true)
        {
            float waveRadius = m_defaultWaveRadius;
            float timePassed = 0.0f;
            scaleFactor = mc_originalScaleFactor;
            m_waveRadius.transform.localScale = new Vector3(waveRadius, waveRadius, waveRadius);
            bool triggered = false;
            while (!Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                //TODO: waving is weird if you wait
                timePassed += Time.deltaTime;
                if (Input.GetMouseButton(1))
                {
                    waving = true;
                    m_animator.SetBool("Down", true);
                    if (triggered == false)
                    {
                        triggered = true;
                        m_audiosource.clip = m_chargeClip;
                        m_audiosource.Play();
                    }
                    waveRadius += Time.deltaTime * scaleFactor;
                    scaleFactor -= Time.deltaTime;
                    m_animator.SetBool("Wave", true);
                    m_animator.SetBool("Down", false);



                    m_waveRadius.transform.localScale = new Vector3(waveRadius, waveRadius, waveRadius);
                    
                }
                else
                {
                    triggered = false;
                    scaleFactor = mc_originalScaleFactor;
                    if (waving)
                    {
                        foreach (Voter voter in GameManager.ms_instance.GetAllVoters())
                        {
                            if (Vector2.Distance(voter.transform.position, transform.position) < waveRadius / 2 + .1f)
                            {
                                m_audiosource.clip = m_deployClip;
                                m_audiosource.Play();
                                voter.ProcessWave();
                            }
                            else
                            {
                                voter.DontGetWavedAt();
                            }
                        }
                        waving = false;
                    }
                    waveRadius = m_defaultWaveRadius;
                    m_waveRadius.transform.localScale = new Vector3(waveRadius, waveRadius, waveRadius);

                }

                interrupted = false;
                yield return new WaitForEndOfFrame();
            }

            foreach (Voter voter in GameManager.ms_instance.GetAllVoters())
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, voter.transform.position - transform.position, waveRadius / 2, m_ignorePlayer);

                if (hit.transform != null && hit.transform.GetComponent<Voter>() == voter)
                {
                    voter.ProcessWave();
                }
                else
                {
                    voter.DontGetWavedAt();
                }
            }

            foreach (Voter voter in GameManager.ms_instance.GetAllVoters())
            {
                voter.DontGetWavedAt();
            }

            m_waveRadius.transform.localScale = new Vector3(.1f, .1f, .1f);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_audiosource.clip = null;
            m_rigidbody.AddForce((new Vector2(worldPos.x, worldPos.y) - new Vector2(transform.position.x, transform.position.y)).normalized * 200000.0f * Time.deltaTime);

            if (transform.position.x > worldPos.x + .1f)
            {

                m_animator.SetBool("Left", true);
                m_animator.SetBool("Right", false);
                m_animator.SetBool("Wave", false);
                m_animator.SetBool("Down", false);

            }
            else if (transform.position.x < worldPos.x - .1f)
            {

                m_animator.SetBool("Left", false);
                m_animator.SetBool("Right", true);
                m_animator.SetBool("Wave", false);
                m_animator.SetBool("Down", false);

            }
            else
            {

                m_animator.SetBool("Left", false);
                m_animator.SetBool("Right", false);
                m_animator.SetBool("Wave", false);
                m_animator.SetBool("Down", true);
            }


            yield return new WaitForEndOfFrame();
        }
    }

    private void KickAwayFromLeader(GameObject go)
    {

        m_rigidbody.AddForce((transform.position - go.transform.position).normalized * MC_PROTESTOR_KICK_RATE);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Voter" && coll.gameObject.GetComponent<Voter>().GetTeam() == Voter.Team.BlueTeam && coll.gameObject.GetComponent<Voter>() is Leader)
        {
            KickAwayFromLeader(coll.gameObject);
        }

    }
    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Voter" && coll.gameObject.GetComponent<Voter>().GetTeam() == Voter.Team.BlueTeam && coll.gameObject.GetComponent<Voter>() is Leader)
        {
            KickAwayFromLeader(coll.gameObject);

        }

    }
}