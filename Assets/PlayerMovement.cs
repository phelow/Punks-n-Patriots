using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovement : Unit
{
    [SerializeField]
    private GameObject m_waveRadius;

    private float m_defaultWaveRadius = .0f;

    [SerializeField]
    private MeshRenderer m_waveSphereRenderer;

    [SerializeField]
    private LayerMask m_ignorePlayer;

    [SerializeField]
    private float scaleFactor = 2.6f;

    private float mc_originalScaleFactor = 3.0f;

    public static PlayerMovement ms_instance;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private AudioClip m_chargeClip;

    [SerializeField]
    private AudioClip m_deployClip;

    private float m_waveEffectRadius;
    private Vector2 m_lastScreenPosition;

    void Awake()
    {
        PlayerPrefs.SetString("LastLevel", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        ms_instance = this;
    }

    // Use this for initialization
    void Start()
    {
        this.StartCoroutine(this.MovePlayer());
    }

    private IEnumerator WaveRoutine()
    {
        m_audiosource.PlayOneShot(m_chargeClip);
        float timePassed = 0.0f;
        while (true)
        {
            if (Input.GetMouseButton(0) ||
                Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f ||
                Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
            {
                yield break;
            }

            //TODO: waving is weird if you wait
            timePassed += Time.deltaTime;
            m_waveSphereRenderer.enabled = true;
            if ((Input.GetMouseButton(1) || Input.GetKey(KeyCode.Space)))
            {
                m_animator.SetBool("Down", true);

                m_waveEffectRadius += Time.deltaTime * scaleFactor;
                scaleFactor -= Time.deltaTime;
                m_animator.SetBool("Wave", true);
                m_animator.SetBool("Down", false);
                
                m_waveRadius.transform.localScale = new Vector3(m_waveEffectRadius, m_waveEffectRadius, m_waveEffectRadius);

            }
            else
            {
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

    }

    private IEnumerator MovePlayer()
    {
        bool waving = false;
        m_waveSphereRenderer.enabled = false;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            m_waveEffectRadius = m_defaultWaveRadius;
            float timePassed = 0.0f;
            scaleFactor = mc_originalScaleFactor;
            m_waveRadius.transform.localScale = new Vector3(m_waveEffectRadius, m_waveEffectRadius, m_waveEffectRadius);
            bool triggered = false;

            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Space))
            {
                yield return this.WaveRoutine();
                m_audiosource.Stop();
                m_waveSphereRenderer.enabled = false;

                float d_radius = m_waveEffectRadius;
                Vector3 d_position = transform.position;
                Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, m_waveEffectRadius / 2);
                bool shouldPlay = false;
                foreach (Collider2D voterCast in collisions)
                {
                    Voter voter = voterCast.transform.GetComponentInChildren<Voter>();
                    if (voter == null)
                    {
                        continue;
                    }

                    shouldPlay = true;
                    voter.ProcessWave();
                }

                if (shouldPlay)
                {

                    m_audiosource.PlayOneShot(m_deployClip);
                }

                m_waveRadius.transform.localScale = new Vector3(.1f, .1f, .1f);
            }

            m_audiosource.clip = null;


            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 movement = new Vector2(0, 0);

            if (Input.GetMouseButton(0))
            {
                movement = (new Vector2(worldPos.x, worldPos.y) - new Vector2(transform.position.x, transform.position.y));
            }

            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
            {
                m_lastScreenPosition = Input.mousePosition;
                worldPos = new Vector3(Input.GetAxis("Horizontal") * 100.0f, Input.GetAxis("Vertical") * 100.0f, 0.0f);
                movement = (new Vector2(worldPos.x, worldPos.y) - new Vector2(transform.position.x, transform.position.y));
            }

            if (movement.magnitude > .03f)
            {
                m_rigidbody.AddForce(movement.normalized * 200000.0f * Time.deltaTime);
            }

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

        }
    }


}