using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class ObjectGridLayout : MonoBehaviour
{
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 4;
    [SerializeField] private float horizontalSpacing = 5.15f;
    [SerializeField] private float verticalSpacing = 5.15f;
    [SerializeField] private float fixedLocalX = 0.58f;
    [SerializeField] private float horizontalStartZ = -0.155f;
    [SerializeField] private float verticalStartY = 0f;
    [SerializeField] private bool sortByButtonText = true;
    [SerializeField] private bool limitToRowsAndColumns = true;

    private void Start()
    {
        ArrangeChildren();
    }

    private void OnValidate()
    {
        ArrangeChildren();
    }

    private void OnTransformChildrenChanged()
    {
        ArrangeChildren();
    }

    [ContextMenu("Arrange Children")]
    private void ArrangeChildren()
    {
        int safeColumns = Mathf.Max(1, columns);
        int safeRows = Mathf.Max(1, rows);
        int itemCount = transform.childCount;
        if (limitToRowsAndColumns)
        {
            itemCount = Mathf.Min(itemCount, safeColumns * safeRows);
        }

        List<Transform> children = GetLayoutChildren(itemCount);
        for (int i = 0; i < children.Count; i++)
        {
            int row = i / safeColumns;
            int column = i % safeColumns;

            Transform child = children[i];

            child.localPosition = new Vector3(
                fixedLocalX,
                verticalStartY - row * verticalSpacing,
                horizontalStartZ + column * horizontalSpacing
            );
        }
    }

    private List<Transform> GetLayoutChildren(int itemCount)
    {
        List<Transform> children = new List<Transform>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        if (sortByButtonText)
        {
            children.Sort((left, right) => GetLayoutOrder(left).CompareTo(GetLayoutOrder(right)));
        }

        return children;
    }

    private static int GetLayoutOrder(Transform child)
    {
        TMP_Text text = child.GetComponentInChildren<TMP_Text>(true);
        string label = text != null ? text.text.Trim() : child.name;

        if (int.TryParse(label, out int digit))
        {
            return digit == 0 ? 10 : digit - 1;
        }

        if (label.Equals("Del", System.StringComparison.OrdinalIgnoreCase))
        {
            return 9;
        }

        if (label.Equals("OK", System.StringComparison.OrdinalIgnoreCase))
        {
            return 11;
        }

        return 100;
    }
}
