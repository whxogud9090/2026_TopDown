using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public AudioSource audioSource;
    public AudioClip pressClip;
    public float pressedScale = 0.94f;
    public float hoverScale = 1.035f;
    public float animationSpeed = 16f;

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, animationSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = baseScale * pressedScale;
        if (audioSource != null && pressClip != null)
            audioSource.PlayOneShot(pressClip);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }
}
