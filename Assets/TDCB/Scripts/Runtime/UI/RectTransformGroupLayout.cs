using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class RectTransformGroupLayout : UIBehaviour, ILayoutElement
{
    public enum LayoutType
    {
        UseBiggest,
        Combine
    }
    
    [SerializeField] private List<RectTransform> targetTransforms;
    [SerializeField] private LayoutType layoutType;
    [SerializeField] private int _layoutPriority;
    
    [System.NonSerialized] private RectTransform m_Rect;
    public RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    public float minWidth { get; private set;}
    public float preferredWidth { get; private set;}
    public float flexibleWidth { get; private set; }
    public float minHeight { get; private set;}
    public float preferredHeight { get; private set;}
    public float flexibleHeight { get; private set;}
    public int layoutPriority => _layoutPriority;

    private void Awake()
    {
        var rect = rectTransform.rect;
        preferredWidth = rect.width;
        preferredHeight = rect.height;
    }

    public void AddTransform(RectTransform targetTransform)
    {
        if (targetTransforms.Contains(targetTransform))
        {
            return;
        }
        
        targetTransforms.Add(targetTransform);
    }
    
    public void RemoveTransform(RectTransform targetTransform)
    {
        targetTransforms.Remove(targetTransform);
    }

    private void OnEnable()
    {
        RecalculateSize();
    }

    [Button]
    public void RecalculateSize()
    {
        CalculateLayoutInputHorizontal();
        CalculateLayoutInputVertical();
    }

    public void CalculateLayoutInputHorizontal()
    {
        minWidth = CalculateSize(0);
        SetDirty();
    }

    public void CalculateLayoutInputVertical()
    {
        minHeight = CalculateSize(1);
        SetDirty();
    }

    private float CalculateSize(int axis)
    {
        float result = 0;
        if (layoutType == LayoutType.Combine)
        {
            foreach (var targetTransform in targetTransforms)
            {
                if(!targetTransform.gameObject.activeInHierarchy) continue;
                result += GetSize(targetTransform, axis);
            }
        }
        else
        {
            foreach (var targetTransform in targetTransforms)
            {
                if(!targetTransform.gameObject.activeInHierarchy) continue;
                result = Mathf.Max(result, GetSize(targetTransform, axis));
            }
        }

        return result;
    }

    private float GetSize(RectTransform targetTransform, int axis)
    {
        return LayoutUtility.GetPreferredSize(targetTransform, axis);
    }

    private void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
    
#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        RecalculateSize();
    }
#endif
}
