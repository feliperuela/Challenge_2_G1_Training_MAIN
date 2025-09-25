using UnityEngine;
using TMPro;
using System.Linq;
using Unity.MLAgents;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag G1_Agent to this field.")]
    public G1_Agent agent;

    [Tooltip("Drag TextMeshPro to this field.")]
    public TextMeshProUGUI metricsText;

    private int episodeCount = 0;
    private int previousStepCount = 0;
    
    private const float ESTABILIDADE_THRESHOLD = 0.0001f;

    void Start()
    {
        if (agent == null || metricsText == null)
        {
            Debug.LogError("HUDManager: The references was not set!");
            this.enabled = false;
            return;
        }
        
        episodeCount = 1;
        UpdateMetrics();
    }

    void Update()
    {
        if (agent.StepCount < previousStepCount)
        {
            episodeCount++;
        }
        previousStepCount = agent.StepCount;

        UpdateMetrics();
    }

    private void UpdateMetrics()
    {
        if (agent == null) return;

        int episodeSteps = agent.StepCount; 
        float cumulativeReward = agent.GetCumulativeReward(); 
        float extrinsicReward = agent.ExtrinsicReward;
        float currentGravity = Physics.gravity.y;
        float uprightBonus = agent.UprightBonus;
        float variacaoEquilibrio = agent.VariacaoDoEquilibrio;

        string statusEquilibrio;
        if (variacaoEquilibrio > ESTABILIDADE_THRESHOLD)
        {
            statusEquilibrio = "Good!";
        }
        else if (variacaoEquilibrio < -ESTABILIDADE_THRESHOLD)
        {
            statusEquilibrio = "Bad!";
        }
        else
        {
            statusEquilibrio = "Estable";
        }

        string actionsString = "Strength: [Aguardando...]";
        if (agent.LastActions != null && agent.LastActions.Length > 0)
        {
            var formattedActions = agent.LastActions.Select(a => a.ToString("F2"));
            actionsString = $"Ações (Strength): [{string.Join(", ", formattedActions)}]";
        }
        
        // Hud text
            string formattedText =
            $"Episode: {episodeCount}\n" +
            $"Steps: {episodeSteps}\n" +
            $"Balance: {uprightBonus:F3}\n" +
            $"Balance - Variation: {variacaoEquilibrio:F4}\n" +
            $"Balance - Status: {statusEquilibrio}\n" +
            $"Reward: {cumulativeReward:F2}\n" + // <-- LINHA REINTRODUZIDA
            $"Step reward: {extrinsicReward:F4}\n" +
            $"---\n" +
            $"Gravity: {currentGravity:F2}\n" +
            $"{actionsString}"; 
        
        metricsText.text = formattedText;
    }
}