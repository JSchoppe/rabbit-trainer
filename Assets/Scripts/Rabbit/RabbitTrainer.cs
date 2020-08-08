using UnityEngine;
using Unity.MLAgents;

/// <summary>Controls the training episodes for rabbit agents</summary>
public sealed class RabbitTrainer : MonoBehaviour
{
    #region Inspector References
    [Tooltip("The environment script that the agents interact with")]
    [SerializeField] private Environment environment = null;
    [Tooltip("The rabbits to train in the environment")]
    [SerializeField] private RabbitAgent[] rabbits = null;
    [Tooltip("A corner of the rabbit spawn boundaries")]
    [SerializeField] private Transform spawnMin = null;
    [Tooltip("The other corner of the rabbit spawn boundaries")]
    [SerializeField] private Transform spawnMax = null;
    #endregion

    private void Start()
    {
        // Listen for when the end of the agents lifespans.
        foreach(RabbitAgent rabbit in rabbits)
            rabbit.OnAgentDeceased += OnAgentDeceased;
        // Start the first episode.
        StartNextEpisode();
    }

    // Once all the agents have passed away, move on to the next episode.
    private int deceasedAgents = 0;
    private void OnAgentDeceased(Agent agent)
    {
        deceasedAgents++;
        if(deceasedAgents == rabbits.Length)
        {
            deceasedAgents = 0;
            StartNextEpisode();
        }
    }

    private void StartNextEpisode()
    {
        // Create a new environment layout.
        environment.Generate();
        // Reset the actor agents.
        foreach(RabbitAgent rabbit in rabbits)
        {
            // Reset the state of the rabbit and place them in a new position.
            rabbit.transform.position = new Vector3(
                Mathf.Lerp(spawnMin.position.x, spawnMax.position.x, Random.value),
                Mathf.Lerp(spawnMin.position.y, spawnMax.position.y, Random.value),
                Mathf.Lerp(spawnMin.position.z, spawnMax.position.z, Random.value)
            );
            rabbit.Respawn();
            // Restart the episode for the agent.
            rabbit.EndEpisode();
        }
    }
}
