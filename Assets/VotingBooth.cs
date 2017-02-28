﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotingBooth : MonoBehaviour {
    void Start()
    {
        GameManager.ms_instance.AddVotingBooth(this);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //check to see if the colliding gameobject is visible. If it is not, return.
        Vector3 viewPosition = Camera.main.WorldToViewportPoint(coll.gameObject.transform.position);

        if(viewPosition.x > 1.0f || viewPosition.x < 0.0f || viewPosition.y < 0.0f || viewPosition.y > 1.0f)
        {
            return;
        }

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
