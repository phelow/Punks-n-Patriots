using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotingBooth : MonoBehaviour {
    void Start()
    {
        GameManager.ms_instance.AddVotingBooth(this);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag == "Voter")
        {
            Voter voter = coll.gameObject.GetComponent<Voter>();

            if (voter.GetTeam() == Voter.Team.BlueTeam)
            {
                voter.CastBlueVote(voter);
            }
            else
            {
                voter.CastRedVote(voter);

            }

            GameManager.ms_instance.RemoveVoter(coll.gameObject.GetComponent<Voter>());
        }
    }
}
