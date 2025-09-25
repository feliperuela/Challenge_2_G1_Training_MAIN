using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// ML-Agent for the Unitree G1 Humanoid Robot.
/// Primary Goal: Achieve and maintain static balance.
/// </summary>
public class G1_Agent : Agent
{
    // --- Public properties for external monitoring (e.g., a custom HUD) ---
    public float[] LastActions { get; private set; }
    public float ExtrinsicReward { get; private set; }
    public float UprightBonus { get; private set; }
    public float VariacaoDoEquilibrio { get; private set; }

    [Header("Robot Ref")]
    public ArticulationBody pelvis;
    public List<ArticulationBody> controllableJoints;
    
    [Header("Control Parameters")]
    public float actionStrength = 10.0f;
    
    [Header("Dynamic Balance Parameters")]
    [Tooltip("Penalty applied when the agent's balance worsens.")]
    public float punicaoPorQueda = -0.05f;
    [Tooltip("The agent is only penalized for negative balance variation if the upright bonus is already below this threshold.")]
    [Range(0.0f, 1.0f)]
    public float limiteBonusParaPunir = 0.95f;

    [Header("Episode Termination Parameters")]
    [Tooltip("If the Upright Bonus falls below this value, the episode ends.")]
    [Range(0.0f, 1.0f)]
    public float limiteDeQueda = 0.8f;
    
    // --- Private variables ---
    private Dictionary<ArticulationBody, Quaternion> initialRotations;
    private Vector3 initialPosition;
    private Quaternion initialRootRotation;
    private StatsRecorder statsRecorder;
    private float previousUprightBonus = 1f;

    /// <summary>
    /// Caches initial positions and rotations for resetting the agent.
    /// Called once when the agent is initialized.
    /// </summary>
    public override void Initialize()
    {
        initialRotations = new Dictionary<ArticulationBody, Quaternion>();
        initialPosition = pelvis.transform.position;
        initialRootRotation = pelvis.transform.rotation;
        foreach (var joint in controllableJoints)
        {
            if(joint != null) // Defensive check to prevent null reference errors
            {
                initialRotations[joint] = joint.transform.localRotation;
            }
        }
        statsRecorder = Academy.Instance.StatsRecorder;
    }
    
    /// <summary>
    /// Resets the agent to its initial state at the beginning of each training episode.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset root body part
        pelvis.TeleportRoot(initialPosition, initialRootRotation);
        pelvis.velocity = Vector3.zero;
        pelvis.angularVelocity = Vector3.zero;

        // Reset all controllable joints
        foreach (var joint in controllableJoints)
        {
            if (joint != null)
            {
                joint.transform.localRotation = initialRotations[joint];
                var drive = joint.xDrive;
                drive.target = 0f;
                joint.xDrive = drive;
                var jointVelocity = new ArticulationReducedSpace(0f);
                joint.jointVelocity = jointVelocity;
            }
        }
        previousUprightBonus = 1f;
    }
    
    /// <summary>
    /// Collects observations from the environment for the agent's policy.
    /// Total observations: 35
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Pelvis orientation (6 observations) - using up and forward vectors is more stable for learning
        sensor.AddObservation(pelvis.transform.up);
        sensor.AddObservation(pelvis.transform.forward);

        // Pelvis angular velocity (3 observations)
        sensor.AddObservation(pelvis.angularVelocity);

        // Joint positions and velocities for all controllable joints (13 joints * 2 = 26 observations)
        foreach (var joint in controllableJoints)
        {
            if (joint != null)
            {
                sensor.AddObservation(joint.jointPosition[0]); // Current normalized position
                sensor.AddObservation(joint.jointVelocity[0]); // Current velocity
            }
        }
    }

    /// <summary>
    /// Receives actions from the policy, applies them to the agent, and calculates the reward.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        LastActions = continuousActions.ToArray(); // For HUD monitoring

        // Apply actions to each joint
        for (int i = 0; i < controllableJoints.Count; i++)
        {
            var joint = controllableJoints[i];
            if (joint != null)
            {
                var drive = joint.xDrive;
                drive.target = continuousActions[i] * actionStrength; 
                joint.xDrive = drive;
            }
        }

        // --- Reward Signal Calculation ---
        // For HUD monitoring
        ExtrinsicReward = 0f;
        UprightBonus = Vector3.Dot(pelvis.transform.up, Vector3.up);
        VariacaoDoEquilibrio = UprightBonus - previousUprightBonus;
        
        AddReward(0.005f);
        ExtrinsicReward += 0.1f; // For HUD
        
        if (UprightBonus < limiteBonusParaPunir && VariacaoDoEquilibrio < 0)
        {
            AddReward(punicaoPorQueda);
            ExtrinsicReward += punicaoPorQueda; // For HUD
        }
        
        // Update the previous balance state for the next step's calculation
        previousUprightBonus = UprightBonus;
        statsRecorder.Add("Agent/UprightBonus", UprightBonus);

        // Terminal condition: end the episode if the agent has fallen
        if (UprightBonus < limiteDeQueda)
        {
            SetReward(-1.0f);
            ExtrinsicReward = -1.0f; // For HUD
            EndEpisode();
        }
    }
}