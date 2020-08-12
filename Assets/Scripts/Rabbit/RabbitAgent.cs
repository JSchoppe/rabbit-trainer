using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public delegate void AgentEventListener(Agent agent);

/// <summary>Represents a rabbit actor in the scene environment</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class RabbitAgent : Agent
{
    #region Inspector References
    [Header("Observation Parameters")]
    [Tooltip("How many meters ahead of itself the agent can see")]
    [SerializeField] private float maxVisionDistance = 10;
    [Tooltip("Directions that the eyes can see in")]
    [SerializeField] private Transform[] visionTrajectories = null;

    [Header("Action Parameters")]
    [Tooltip("The minimum hop force the rabbit can exert")]
    [SerializeField] private float minHopVelocity = 1;
    [Tooltip("The maximum hop force the rabbit can exert")]
    [SerializeField] private float maxHopVelocity = 2;
    [Tooltip("Minimum grounded wait time before another hop")]
    [SerializeField] private float minHopDelay = 1;
    [Tooltip("Maximum grounded wait time before another hop")]
    [SerializeField] private float maxHopDelay = 2;
    [Tooltip("Direction of the hop force relative to the body")]
    [SerializeField] private Transform hopTrajectory = null;

    [Header("Fitness Parameters")]
    [Tooltip("The rate at which units of food are consumed per second")]
    [SerializeField] private float consumptionRate = 1;
    [Tooltip("The initial energy of the rabbit")]
    [SerializeField] private float startingEnergy = 50;
    [Tooltip("How much energy is lost per second")]
    [SerializeField] private float homeostasisEnergyLoss = 1;
    [Tooltip("Process that describe how food is metabolized into energy")]
    [SerializeField] private MetabolicProcess[] metabolicProcesses = null;

    [Header("Training Parameters")]
    [Tooltip("The environment the rabbit is in(strictly for reward purposes)")]
    [SerializeField] private Environment environment = null;
    [Tooltip("Reward given per second when a rabbit is consuming an item")]
    [SerializeField] private float consumptionReward = 0.1f;
    [Tooltip("Reward given when the agent is near an environment consumable")]
    [SerializeField] private float proximityReward = 0.1f;
    [Tooltip("Range in meters where the proximity reward falls off to zero")]
    [SerializeField] private float proximityRange = 1;
    #endregion
    private Rigidbody body;

    // Fitness runtime variables.
    private Dictionary<ConsumableType, float> stomach;
    private bool isAlive = false;
    private bool isEating = false;
    private float currentEnergy;
    private Consumable currentConsumable;

    /// <summary>Called when this agent runs out of energy</summary>
    public event AgentEventListener OnAgentDeceased;

    #region Gizmo Implementation
    // Draw each vision line of the agent.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Transform trajectory in visionTrajectories)
        {
            Vector3 unitTrajectory = (trajectory.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + unitTrajectory * maxVisionDistance);
        }
    }
    #endregion
    #region Rabbit Implementation
    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    /// <summary>Resets the rabbit to a healthy state</summary>
    public void Respawn()
    {
        // Create an empty stomach to store food types for digestion.
        stomach = new Dictionary<ConsumableType, float>();
        foreach(byte value in System.Enum.GetValues(typeof(ConsumableType)))
            stomach.Add((ConsumableType)value, 0);

        // Reset physics state.
        body.velocity = Vector3.zero;
        // Reset the rabbit state and energy.
        isAlive = true;
        currentEnergy = startingEnergy;
    }

    /// <summary>Prompts the agent to eat an item they are close to</summary>
    /// <param name="consumable">The consumable item</param>
    public void Eat(Consumable consumable)
    {
        float consumption = Time.fixedDeltaTime * consumptionRate;
        if(consumable.ConsumableUnits < consumption)
        {
            consumption = consumable.ConsumableUnits;
            consumable.ConsumableUnits = 0;
            isEating = false;
        }
        else
        {
            consumable.ConsumableUnits -= consumption;
        }
        stomach[consumable.ConsumableType] += consumption;

        // Hard code a reward to encourage eating objects.
        AddReward(Time.fixedDeltaTime * consumptionReward);
    }
    /// <summary>Simulates metabolism of food over time</summary>
    private void Metabolize()
    {
        // Simulate digestion reactions:
        foreach(MetabolicProcess process in metabolicProcesses)
        {
            // Check to see if every component is available for this process.
            bool allComponentsAvailable = true;
            foreach(KeyValuePair<ConsumableType, float> requirement in process.Requirements)
            {
                if(stomach[requirement.Key] < requirement.Value * Time.fixedDeltaTime)
                {
                    allComponentsAvailable = false;
                    break;
                }
            }
            if(allComponentsAvailable)
            {
                // Actually run this metabolic process.
                foreach(KeyValuePair<ConsumableType, float> requirement in process.Requirements)
                {
                    stomach[requirement.Key] -= requirement.Value * Time.fixedDeltaTime;
                    currentEnergy += process.EnergyProduced * Time.fixedDeltaTime;
                }
            }
        }

        // Control energy loss.
        currentEnergy -= homeostasisEnergyLoss * Time.fixedDeltaTime;
        if(currentEnergy < 0)
        {
            // Finalize state and notify listeners.
            isAlive = false;
            OnAgentDeceased?.Invoke(this);
        }
    }
    #endregion
    #region Agent Implementation
    private void FixedUpdate()
    {
        if(isAlive)
            Metabolize();
    }

    // When the agent collides with a surface:
    private void OnCollisionEnter(Collision collision)
    {
        if(isAlive)
        {
            if(collision.transform.CompareTag("Floor"))
            {
                // Assign a reward based on the proximity of consumables.
                foreach(Consumable consumable in environment.spawnedConsumables)
                {
                    float distance = Vector3.Distance(consumable.transform.position, transform.position);
                    if(distance < proximityRange)
                    {
                        // Create a reward proportional to the proximity of this consumable.
                        float reward = Mathf.Lerp(proximityReward, 0, Mathf.InverseLerp(0, proximityRange, distance));
                        // Increase reward of the consumables in proximity have more to eat.
                        reward *= consumable.ConsumableUnits;
                        AddReward(reward);
                    }
                }
                // Request the next action since the rabbit has become grounded.
                RequestDecision();
            }
            else if(collision.transform.CompareTag("Consumable"))
            {
                // Prompt the rabbit to eat this consumable.
                isEating = true;
                currentConsumable = collision.gameObject.GetComponent<Consumable>();
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(isAlive && collision.transform.CompareTag("Consumable"))
            isEating = false;
    }

    // Observation space explanation
    // [0, 1] : Is eating
    // [2, 3, 4] : Food in stomach(represents fullness)
    // Where n is from 0 to the number of raycasts:
    //    [5 + 4n] : Vision raycast object(one hot observation)
    //       Nothing
    //       Ground
    //       Rabbit Agent
    //       Grass
    //       Berry
    //       Poison Berry
    //    [6 + 4n] : Quantity of the consumable item
    //    [7 + 4n] : Distance of object from rabbit
    //    [8 + 4n] : Angle of object relative to rabbit
    public override void CollectObservations(VectorSensor sensor)
    {
        // Allow the rabbit to develop hunger signals based on the
        // contents of their stomach.
        sensor.AddOneHotObservation(isEating? 1 : 0, 2);
        sensor.AddObservation(stomach[ConsumableType.Peppers]);
        sensor.AddObservation(stomach[ConsumableType.Grass]);
        sensor.AddObservation(stomach[ConsumableType.Lettuce]);

        foreach(Transform visionTrajectory in visionTrajectories)
        {
            Vector3 castDirection = maxVisionDistance * (visionTrajectory.position - transform.position).normalized;
            if(Physics.Linecast(transform.position, transform.position + castDirection, out RaycastHit hit))
            {
                switch(hit.collider.tag)
                {
                    case "Consumable":
                        Debug.DrawLine(transform.position, hit.point, Color.green, 1);
                        // Retrieve the consumable script and observe its properties.
                        Consumable item = hit.collider.gameObject.GetComponent<Consumable>();
                        sensor.AddOneHotObservation((int)item.ConsumableType, 6);
                        sensor.AddObservation(item.ConsumableUnits);
                        break;
                    case "Ground":
                        Debug.DrawLine(transform.position, hit.point, Color.black, 1);
                        sensor.AddOneHotObservation(3, 6);
                        sensor.AddObservation(0);
                        break;
                    case "Agent":
                        Debug.DrawLine(transform.position, hit.point, Color.white, 1);
                        sensor.AddOneHotObservation(4, 6);
                        sensor.AddObservation(0);
                        break;
                    default:
                        sensor.AddOneHotObservation(5, 6);
                        sensor.AddObservation(0);
                        break;
                }
                // Where is this object in relation to the rabbit?
                Vector3 relativeLocation = transform.InverseTransformPoint(hit.transform.position);
                // Depth of this object from the rabbit.
                sensor.AddObservation(relativeLocation.magnitude);
                // Get the angle of the observed object.
                sensor.AddObservation(Vector3.SignedAngle(Vector3.forward, relativeLocation, Vector3.up) / 180f);
            }
            else
            {
                // Case where raycast hits nothing.
                sensor.AddOneHotObservation(5, 6);
                sensor.AddObservation(0);
                sensor.AddObservation(1);
                sensor.AddObservation(1);
            }
        }
    }

    // Action space explanation
    // Requested every frame the rabbit is grounded
    // actions[0] : How many degrees should the rabbit turn?
    //    -1 : -180
    //     1 : +180
    // actions[1] : How long before the rabbit hops?
    //    -1 : minHopDelay
    //     1 : maxHopDelay
    // actions[2] : What is the hop force?
    //    -1 : minHopForce
    //     1 : maxHopForce
    public override void OnActionReceived(float[] actions)
    {
        transform.Rotate(Vector3.up, Mathf.Clamp(actions[0], -1, 1) * 180);
        float delay = Mathf.Lerp(minHopDelay, maxHopDelay, Mathf.InverseLerp(-1, 1, actions[1]));
        float hopForce = Mathf.Lerp(minHopVelocity, maxHopVelocity, Mathf.InverseLerp(-1, 1, actions[2]));
        StartCoroutine(ScheduleHop(hopForce, delay));
    }
    private IEnumerator ScheduleHop(float hopForce, float delay)
    {
        // Wait according to the agents demand.
        yield return new WaitForSeconds(delay);
        // Apply the jump force.
        Vector3 direction = (hopTrajectory.position - transform.position).normalized;
        body.AddForce(direction * hopForce, ForceMode.VelocityChange);
    }
    #endregion
}
