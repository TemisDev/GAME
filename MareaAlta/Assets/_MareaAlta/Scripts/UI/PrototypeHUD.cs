using MareaAlta.Resources;
using MareaAlta.Interaction;
using MareaAlta.Core;
using MareaAlta.World;
using UnityEngine;

namespace MareaAlta.UI
{
    [RequireComponent(typeof(EnergyInventory))]
    public sealed class PrototypeHUD : MonoBehaviour
    {
        private EnergyInventory inventory;
        private PlayerInteractor interactor;
        private GameSession gameSession;
        private TurbineController[] turbines;
        private Transform playerTransform;
        private GUIStyle headerStyle;
        private GUIStyle valueStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle promptStyle;

        private int totalDebris;

        private void Awake()
        {
            inventory = GetComponent<EnergyInventory>();
            interactor = GetComponent<PlayerInteractor>();
            gameSession = FindFirstObjectByType<GameSession>();
            turbines = FindObjectsByType<TurbineController>(FindObjectsSortMode.None);
            System.Array.Sort(turbines, (left, right) => left.transform.position.x.CompareTo(right.transform.position.x));
            totalDebris = Mathf.Max(1, FindObjectsByType<DebrisCollectible>(FindObjectsSortMode.None).Length);
            playerTransform = transform;
        }

        private void OnGUI()
        {
            if (gameSession != null && gameSession.State == GameSession.SessionState.Victory)
                return;

            headerStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.4f, 0.95f, 1f) }
            };
            valueStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            objectiveStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.7f, 1f, 0.82f) }
            };
            promptStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            Rect resourcePanel = new(20f, 20f, 330f, 142f);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.015f, 0.1f, 0.2f, 0.9f);
            GUI.Box(resourcePanel, GUIContent.none);
            GUI.color = previousColor;

            GUI.Label(new Rect(38f, 28f, 290f, 26f), "RECURSOS RECUPERADOS", headerStyle);
            GUI.Label(
                new Rect(38f, 55f, 290f, 58f),
                $"Fragmentos  {inventory.FragmentCount}/{inventory.FragmentsPerEnergy}\nEnergía       {inventory.EnergyCount}",
                valueStyle);

            float progress = Mathf.Clamp01(inventory.TotalFragmentsCollected / (float)totalDebris);
            Rect barBackground = new(38f, 119f, 290f, 18f);
            GUI.color = new Color(0.05f, 0.18f, 0.28f, 1f);
            GUI.DrawTexture(barBackground, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.9f, 0.7f, 1f);
            GUI.DrawTexture(new Rect(barBackground.x, barBackground.y, barBackground.width * progress, barBackground.height), Texture2D.whiteTexture);
            GUI.color = previousColor;

            GUI.Label(
                new Rect((Screen.width - 520f) * 0.5f, Screen.height - 64f, 520f, 42f),
                $"{GetZoneName()}  •  {GetCurrentObjective()}",
                objectiveStyle);

            if (interactor != null && !string.IsNullOrEmpty(interactor.InteractionPrompt))
            {
                GUI.Box(
                    new Rect((Screen.width - 430f) * 0.5f, Screen.height - 120f, 430f, 48f),
                    interactor.InteractionPrompt,
                    promptStyle);
            }
        }

        private string GetCurrentObjective()
        {
            TurbineController pending = GetPendingTurbine();
            if (pending == null)
                return "OBJETIVO: alcanza la baliza de salida";
            if (inventory.EnergyCount > 0)
                return "OBJETIVO: encuentra la turbina y actívala con E";

            int remaining = inventory.FragmentsPerEnergy - inventory.FragmentCount;
            return $"OBJETIVO: recoge {remaining} residuo{(remaining == 1 ? string.Empty : "s")} para generar energía";
        }

        private TurbineController GetPendingTurbine()
        {
            if (turbines == null)
                return null;

            foreach (TurbineController candidate in turbines)
            {
                if (candidate != null && !candidate.IsActivated)
                    return candidate;
            }

            return null;
        }

        private string GetZoneName()
        {
            float x = playerTransform != null ? playerTransform.position.x : 0f;
            if (x < 13f)
                return "ENTRADA DEL ARRECIFE";
            if (x < 39f)
                return "RUINAS HUNDIDAS";
            return "FOSA DE SALIDA";
        }
    }
}
