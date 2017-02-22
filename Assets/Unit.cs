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
    protected GameObject m_animatorRed;

    [SerializeField]
    protected GameObject m_animatorBlue;


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

        if(m_team == Team.BlueTeam)
        {
            force += GameManager.ms_instance.GetAllVoters().Count * 10;
        }

        Vector3 movement = (new Vector2(target.x, target.y) - new Vector2(transform.position.x, transform.position.y)).normalized * force;

        float t = 0.0f;

        Vector3 current = movement;

        if (!this is PlayerMovement)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, movement.normalized, 2.0f, m_ignoreVotersMask);
            if (hit.collider != null && hit.collider.gameObject.name != "Polling Station")
            {
                current = Vector2.Reflect(current, Vector2.right);
            }
            Debug.DrawRay(transform.position, current, Color.black, 1.0f);
            Debug.DrawRay(transform.position, movement / 500.0f, Color.red, 1.0f);

        }

        if (m_animatorBlue != null)
        {
            //Debug.Log(target.x + " " + transform.position.x);

            if (target.x > transform.position.x + .1f)
            {
                //Debug.Log("57");
                m_animatorRed.GetComponent<Animator>().SetBool("Right", true);
                m_animatorBlue.GetComponent<Animator>().SetBool("Right", true);
            }
            else if (target.x < transform.position.x - .1f)
            {
                //Debug.Log("62");
                m_animatorRed.GetComponent<Animator>().SetBool("Left", true);
                m_animatorBlue.GetComponent<Animator>().SetBool("Left", true);
            }
        }
        m_rigidbody.AddForce(current);
    }
    
}
