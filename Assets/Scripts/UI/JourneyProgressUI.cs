using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a UI progress bar (Slider or Image fill) to the ship's journey progress.
/// Subscribes to SimpleShipController events to keep UI in sync and provide basic state feedback.
/// </summary>
public class JourneyProgressUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SimpleShipController ship; // If null and autoFind = true, will be found at runtime
    [SerializeField] private bool autoFindShipOnStart = true;

    [Header("UI References")] 
    [Tooltip("Optional Slider. If assigned, its value will be set to progress (0..1)")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("Optional Image with Fill Method. If assigned, fillAmount will be set to progress (0..1)")]
    [SerializeField] private Image progressFillImage;

    [Tooltip("Optional Text to display percentage (0..100%)")] 
    [SerializeField] private Text progressText;

    [Header("Visuals")] 
    [SerializeField] private Color activeColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color stoppedColor = new Color(0.8f, 0.4f, 0.2f);
    [SerializeField] private Color completedColor = new Color(0.2f, 0.6f, 1f);

    private void Start()
    {
        if (ship == null && autoFindShipOnStart)
        {
            ship = FindObjectOfType<SimpleShipController>();
        }

        Subscribe(true);

        // Initialize UI with current state
        if (ship != null)
        {
            UpdateProgress(ship.GetJourneyProgressPercentage());
            ApplyStateVisuals();
        }
    }

    private void OnEnable()
    {
        Subscribe(true);
    }

    private void OnDisable()
    {
        Subscribe(false);
    }

    private void Subscribe(bool subscribe)
    {
        if (ship == null) return;

        if (subscribe)
        {
            ship.OnJourneyProgressChanged?.AddListener(HandleProgressChanged);
            ship.OnJourneyStarted?.AddListener(HandleJourneyStarted);
            ship.OnJourneyStopped?.AddListener(HandleJourneyStopped);
            ship.OnJourneyCompleted?.AddListener(HandleJourneyCompleted);
        }
        else
        {
            ship.OnJourneyProgressChanged?.RemoveListener(HandleProgressChanged);
            ship.OnJourneyStarted?.RemoveListener(HandleJourneyStarted);
            ship.OnJourneyStopped?.RemoveListener(HandleJourneyStopped);
            ship.OnJourneyCompleted?.RemoveListener(HandleJourneyCompleted);
        }
    }

    private void HandleProgressChanged(float percent)
    {
        UpdateProgress(percent);
    }

    private void HandleJourneyStarted()
    {
        ApplyStateVisuals();
    }

    private void HandleJourneyStopped()
    {
        ApplyStateVisuals();
    }

    private void HandleJourneyCompleted()
    {
        ApplyStateVisuals();
    }

    private void UpdateProgress(float percent)
    {
        float normalized = Mathf.Clamp01(percent / 100f);

        if (progressSlider != null)
        {
            progressSlider.normalizedValue = normalized;
        }

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = normalized;
        }

        if (progressText != null)
        {
            progressText.text = Mathf.RoundToInt(percent).ToString() + "%";
        }
    }

    private void ApplyStateVisuals()
    {
        Color color = stoppedColor;
        if (ship != null)
        {
            if (ship.IsJourneyCompleted()) color = completedColor;
            else if (ship.IsJourneyActive()) color = activeColor;
        }

        if (progressFillImage != null)
        {
            progressFillImage.color = color;
        }

        if (progressSlider != null)
        {
            var fill = progressSlider.fillRect != null ? progressSlider.fillRect.GetComponent<Image>() : null;
            if (fill != null) fill.color = color;
        }

        if (progressText != null)
        {
            progressText.color = color;
        }
    }
}
