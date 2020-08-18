using System.Collections;
using UnityEngine;

// The script sidesteps direct parenting so that the camera can exist without
// reorganizing the hierarchy.

/// <summary>Simple camera script for recording the agent</summary>
[RequireComponent(typeof(Camera))]
public sealed class DemoCam : MonoBehaviour
{
    [SerializeField] private bool isFollowingAgent = true;

    [SerializeField] private Transform targetAgent = null;
    [SerializeField] private float distanceBehind = 6;
    [SerializeField] private float distanceAbove = 3;

    private void Start()
    {
        if(isFollowingAgent)
            StartCoroutine(TrackCamera());
    }

    private IEnumerator TrackCamera()
    {
        while(true)
        {
            transform.position = targetAgent.position - targetAgent.forward * distanceBehind + targetAgent.up * distanceAbove;
            transform.LookAt(targetAgent);
            yield return null;
        }
    }
}
