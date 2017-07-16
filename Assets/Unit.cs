using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    [SerializeField]
    protected Rigidbody2D m_rigidbody;

    private float m_movementForce = 500.0f;
    public static float ms_detectionRadius = 20.0f;

    [SerializeField]
    protected LayerMask m_ignoreVotersMask;

    [SerializeField]
    protected Animator m_animatorRed;

    [SerializeField]
    protected Animator m_animatorBlue;

    [SerializeField]
    protected AudioSource m_audiosource;

    [SerializeField]
    public Team m_team;

    public enum Team
    {
        RedTeam,
        BlueTeam
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void MoveTo(Vector3 target, float force = -1.0f)
    {
        if (force == -1.0f)
        {
            force = m_movementForce;
        }

        force += Mathf.Max(GameManager.ms_instance.GetAllVoters().Count * 2, 200);

        Vector3 movement = (new Vector2(target.x, target.y) - new Vector2(transform.position.x, transform.position.y)).normalized * force;
        Debug.DrawRay(transform.position, target - transform.position, Color.blue, 1.0f);
        m_rigidbody.AddForce(movement);

        if (m_animatorBlue != null)
        {
            if (target.x > transform.position.x + .1f)
            {
                //Debug.Log("57");
                if (m_animatorRed.gameObject.activeSelf)
                {
                    m_animatorRed.SetBool("Right", true);
                }
                if (m_animatorBlue.gameObject.activeSelf)
                {
                    m_animatorBlue.SetBool("Right", true);
                }
            }
            else if (target.x < transform.position.x - .1f)
            {
                if (m_animatorRed.gameObject.activeSelf)
                {
                    m_animatorRed.SetBool("Left", true);
                }
                if (m_animatorBlue.gameObject.activeSelf)
                {
                    m_animatorBlue.SetBool("Left", true);
                }
            }
        }
    }

}
