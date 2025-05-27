using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class OS_MoverObject : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool keepWithinCanvas = true;
    
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    private RectTransform canvasRectTransform;
    
    private void Awake()
    {
        if (dragRectTransform == null)
            dragRectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        dragRectTransform.SetAsLastSibling();
        
        if (keepWithinCanvas && canvasRectTransform != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out originalLocalPointerPosition);
            
            originalPanelLocalPosition = dragRectTransform.localPosition;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragRectTransform == null || canvas == null)
            return;
        
        if (keepWithinCanvas && canvasRectTransform != null)
        {
            // Ограниченное перемещение в пределах Canvas
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition))
            {
                Vector2 offset = localPointerPosition - originalLocalPointerPosition;
                dragRectTransform.localPosition = originalPanelLocalPosition + (Vector3)offset;
                
                ClampToCanvas();
            }
        }
        else
        {
            // Простое перемещение без ограничений
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
            {
                dragRectTransform.localPosition = localPoint;
            }
        }
    }
    
    private void ClampToCanvas()
    {
        Vector3[] canvasCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners);
        
        Vector3[] panelCorners = new Vector3[4];
        dragRectTransform.GetWorldCorners(panelCorners);
        
        float canvasMinX = canvasCorners[0].x;
        float canvasMaxX = canvasCorners[2].x;
        float canvasMinY = canvasCorners[0].y;
        float canvasMaxY = canvasCorners[2].y;
        
        float panelWidth = panelCorners[2].x - panelCorners[0].x;
        float panelHeight = panelCorners[2].y - panelCorners[0].y;
        
        float minX = canvasMinX + panelWidth / 2;
        float maxX = canvasMaxX - panelWidth / 2;
        float minY = canvasMinY + panelHeight / 2;
        float maxY = canvasMaxY - panelHeight / 2;
        
        Vector3 panelPosition = dragRectTransform.position;
        panelPosition.x = Mathf.Clamp(panelPosition.x, minX, maxX);
        panelPosition.y = Mathf.Clamp(panelPosition.y, minY, maxY);
        
        dragRectTransform.position = panelPosition;
    }
}