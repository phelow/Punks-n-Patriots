using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster : MonoBehaviour {

    protected List<Voter> m_members;
    protected int m_targetSize;
    [SerializeField]
    private List<Slot> m_availableSlots;
    private float m_distanceForConversion = 2.0f;


    private const int mc_lineRequirement = 5;
    
    public void StartCluster(){
        m_targetSize = UnityEngine.Random.Range(2,3);
        m_members = new List<Voter>();
    }

    public Slot AddMember(Voter member)
    {
        if(m_availableSlots.Count == 0)
        {
            return null;
        }

        m_members.Add(member);
        Slot target = m_availableSlots[Random.Range(0, m_availableSlots.Count)];
        m_availableSlots.Remove(target);



        StartCoroutine(DelayTurnColor());


        m_availableSlots.AddRange(member.GetSlots());
        return target;
    }

    private IEnumerator DelayTurnColor()
    {
        float redTeams = 0;
        float blueTeams = 0;
        float averageDistance = m_distanceForConversion;

        while(averageDistance >= m_distanceForConversion)
        {
            averageDistance = 0.0f;
            float maxDist = 0.0f;

            foreach(Voter voter in m_members)
            {
                if(voter == null)
                {
                    continue;
                }

                float dist = Vector2.Distance(transform.position, voter.transform.position);

                if(dist > maxDist)
                {
                    maxDist = dist;
                }

                averageDistance += dist/m_members.Count;
            }

            if(m_members.Count < 3)
            {
                maxDist = 0;
            }

            yield return new WaitForSeconds(1.0f);
        }

        foreach (Voter voter in m_members)
        {
            if (voter.GetTeam() == Voter.Team.BlueTeam)
            {
                blueTeams +=voter.GetHealth();
            }
            else
            {
                redTeams += voter.GetHealth();
            }
        }
        if (redTeams > blueTeams)
        {
            foreach (Voter voter in m_members)
            {
                if (!voter.IsLeader())
                {
                    voter.TurnRed();
                }
            }
        }
        else if (redTeams < blueTeams)
        {
            foreach (Voter voter in m_members)
            {
                if (!voter.IsLeader())
                {
                    voter.TurnBlue();
                }
            }
        }
    }

    public bool HasRoom()
    {
        return m_members.Count < m_targetSize;
    }

    public Vector3 GetCenter()
    {
        return transform.position;
    }


    public void RemoveMember(Voter member)
    {
        if(member == null)
        {
            return;
        }

        m_members.Remove(member);
        member.KickFromCluster();
    }

    public IEnumerator TickDown()
    {
        while(m_targetSize > 0)
        {
            m_targetSize--;

            if (m_targetSize < m_members.Count)
            {

                Voter furthest = null;
                //Remove the furthest away member
                foreach(Voter voter in m_members)
                {
                    if(voter == null)
                    {
                        continue;
                    }

                    if(furthest == null)
                    {
                        furthest = voter;
                    }

                    if(Vector2.Distance(voter.transform.position,transform.position) > Vector2.Distance(furthest.transform.position, transform.position))
                    {
                        furthest = voter;
                    }
                }
                RemoveMember(furthest);

                yield return new WaitForSeconds(3.0f);

            }
            yield return new WaitForSeconds(1.0f);
        }

        GameManager.ms_instance.RemoveCluster(this);
    }

}
