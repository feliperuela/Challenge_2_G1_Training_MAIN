using UnityEngine;
using TMPro;
using Unity.MLAgents; 
using System.Collections.Generic; // Required to use Lists

/// <summary>
/// A helper class to define curriculum stages in the Unity Inspector.
/// The [System.Serializable] attribute allows instances of this class to be edited in the Inspector.
/// </summary>
[System.Serializable]
public class EstagioDoCurriculo
{
    public string nomeDoEstagio; 
    public long passosParaAtivar;
    public float valorDaGravidade;
}

/// <summary>
/// Manages the scene's gravity automatically based on the total training steps.
/// This implements an automated Curriculum Learning strategy, increasing the task difficulty as the agent learns.
/// </summary>
public class GravityController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your TextMeshPro UI text object here to display the current gravity.")]
    public TextMeshProUGUI gravityValueText;

    [Header("Currículo de Gravidade")]
    [Tooltip("Define the difficulty stages. Gravity will change when the 'Total Steps' reach the activation value for each stage.")]
    public List<EstagioDoCurriculo> curriculoDeGravidade;

    private float gravidadeAtual = 9.81f;

    void Start()
    {
        // Sort the curriculum list to ensure stages are processed in the correct order of steps.
        // This is a crucial step for the logic to work reliably.
        if (curriculoDeGravidade != null)
        {
            curriculoDeGravidade.Sort((a, b) => a.passosParaAtivar.CompareTo(b.passosParaAtivar));
        }
        
        // Apply the initial gravity setting on startup.
        VerificarEAtualizarGravidade();
    }

    // I used FixedUpdate to stay in sync with the ML-Agents physics and step cycle.
    void FixedUpdate()
    {
        VerificarEAtualizarGravidade();
    }

    private void VerificarEAtualizarGravidade()
    {
        // If the Academy is not initialized (e.g., running the scene without mlagents-learn), do nothing.
        if (!Academy.IsInitialized) return;

        long passosTotais = Academy.Instance.TotalStepCount;
        float novaGravidade = 9.81f; // Valor padrão

        // Find the correct curriculum stage for the current total step count.
        if (curriculoDeGravidade != null)
        {
            foreach (var estagio in curriculoDeGravidade)
            {
                if (passosTotais >= estagio.passosParaAtivar)
                {
                    novaGravidade = estagio.valorDaGravidade;
                }
                else
                {
                    // Since the list is sorted, we can stop as soon as we find a stage that has not yet been reached.
                    break;
                }
            }
        }

        // Apply the new gravity setting only if it has changed, for performance optimization.
        if (Mathf.Approximately(gravidadeAtual, novaGravidade) == false)
        {
            gravidadeAtual = novaGravidade;
            Physics.gravity = new Vector3(0, -gravidadeAtual, 0);
        }

        // Update the HUD text, if a reference exists.
        if (gravityValueText != null)
        {
            gravityValueText.text = $"Gravidade: {-gravidadeAtual:F2}";
        }
    }
}