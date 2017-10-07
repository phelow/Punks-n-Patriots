﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> mp_voterPrefabs;

    private int m_maxEnemiesIterator = 2;

    [SerializeField]
    private List<int> m_maxEnemiesOverTime;

    [SerializeField]
    private List<int> _disabledDuringRush;
    private Stack<int> _disabledDuringRushStack;

    private int m_maxEnemies = 0;

    [SerializeField]
    private List<Voter> m_myVoters;

    private const float c_offScreenShutoffTime = 14.0f;
    private const float c_onScreenTurnOnTime = 3.0f;

    bool enabled = true;

    float timeDisabled = 0.0f;

    // Use this for initialization
    void Start()
    {
        _disabledDuringRushStack = new Stack<int>();

        for(int i = _disabledDuringRush.Count-1; i >= 0; i--)
        {
            _disabledDuringRushStack.Push(_disabledDuringRush[i]);
        }

        m_myVoters = new List<Voter>();
        SetMaxEnemiesNext();
        StartCoroutine(SpawnEnemies());
        StartCoroutine(CheckVisible());
    }

    private IEnumerator CheckVisible()
    {
        float timeOnScreen = 0.0f;
        float timeOffScreen = 0.0f;
        while (true)
        {
            //If 10% or more off screen

            Vector3 viewPosition = Camera.main.WorldToViewportPoint(transform.position);

            if ((viewPosition.x > 1.56f || viewPosition.x < -.56f || viewPosition.y < -0.56f || viewPosition.y > 1.56f))
            {
                timeOnScreen = 0.0f;
                timeOffScreen += Time.deltaTime;
            }
            else
            {
                timeOnScreen += Time.deltaTime;
                timeOffScreen = 0.0f;
            }

            if(timeOnScreen > c_offScreenShutoffTime)
            {
                enabled = false;
            }

            if(timeOffScreen > c_onScreenTurnOnTime || GameManager.IsFinalRush())
            {
                enabled = true;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void SetMaxEnemiesNext()
    {
        m_maxEnemies = m_maxEnemiesOverTime[m_maxEnemiesIterator];
        m_maxEnemiesIterator++;
    }


    private IEnumerator SpawnEnemies()
    {
        // for the first thirty seconds only spawn regular protestors
        float timeLeft = 30.0f;

        while (m_maxEnemies <= 0)
        {
            while (_disabledDuringRushStack.Count > 0 && GameManager.GetTimeLeft() < _disabledDuringRushStack.Peek())
            {
                _disabledDuringRushStack.Pop();
                yield return new WaitForSeconds(15.0f);
            }
            yield return new WaitForSeconds(1.0f);
            timeLeft -= 1.0f;
        }

        while (timeLeft > 0.0f)
        {

            while (_disabledDuringRushStack.Count > 0 && GameManager.GetTimeLeft() < _disabledDuringRushStack.Peek())
            {
                _disabledDuringRushStack.Pop();
                yield return new WaitForSeconds(15.0f);
            }
            while (enabled == false)
            {
                yield return new WaitForEndOfFrame();
            }

            Voter voter = GameObject.Instantiate(mp_voterPrefabs[0], transform.position, transform.rotation, null).GetComponent<Voter>();
            m_myVoters.Add(voter);
            GameManager.ms_instance.AddVoter(voter);
            float deltaTime = Random.Range(.8f, 3.5f);
            timeLeft -= deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }

        while (true)
        {

            while (_disabledDuringRushStack.Count > 0 && GameManager.GetTimeLeft() < _disabledDuringRushStack.Peek())
            {
                _disabledDuringRushStack.Pop();
                yield return new WaitForSeconds(15.0f);
            }
            while (enabled == false)
            {
                yield return new WaitForEndOfFrame();
            }

            m_myVoters.RemoveAll(item => item == null);
            m_myVoters.RemoveAll(v => v.GetTeam() == Unit.Team.RedTeam);

            if (m_myVoters.Count > m_maxEnemies)
            {

                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                Voter voter = null;
                if (GameManager.IsFinalRush())
                {
                    voter = GameObject.Instantiate(mp_voterPrefabs[0], transform.position, transform.rotation, null).GetComponent<Voter>();
                }
                else
                {
                    voter = GameObject.Instantiate(mp_voterPrefabs[Random.Range(0, mp_voterPrefabs.Count)], transform.position, transform.rotation, null).GetComponent<Voter>();
                }

                m_myVoters.Add(voter);
                GameManager.ms_instance.AddVoter(voter);

                if (GameManager.IsFinalRush())
                {
                    voter = GameObject.Instantiate(mp_voterPrefabs[Random.Range(0, mp_voterPrefabs.Count)], transform.position, transform.rotation, null).GetComponent<Voter>();
                    m_myVoters.Add(voter);
                    GameManager.ms_instance.AddVoter(voter);
                }

                yield return new WaitForSeconds(Random.Range(.8f, 3.5f));
            }

            while (_disabledDuringRushStack.Count > 0 && GameManager.GetTimeLeft() < _disabledDuringRushStack.Peek())
            {
                _disabledDuringRushStack.Pop();
                yield return new WaitForSeconds(15.0f);
            }
        }
    }

}
