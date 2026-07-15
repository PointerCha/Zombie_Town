using UnityEngine;
using System;

public sealed class UnitInputController : MonoBehaviour
{
    private const int SelectionBufferSize = 16;

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask unitLayerMask = 1 << 6;

    private readonly Collider2D[] selectionHits = new Collider2D[SelectionBufferSize];
    private UnitController selectedUnit;
    private ContactFilter2D selectionFilter;

    public event Action<UnitController> SelectedUnitChanged;

    public UnitController SelectedUnit => selectedUnit;

    private void Awake()
    {
        if (sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }

        selectionFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true
        };
        selectionFilter.SetLayerMask(unitLayerMask);
    }

    private void Update()
    {
        if (sceneCamera == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleMoveCommand();
        }
    }

    private void HandleSelection()
    {
        Vector2 worldPoint = GetMouseWorldPoint();
        SelectUnit(GetSelectableUnitAt(worldPoint));
    }

    private void HandleMoveCommand()
    {
        if (selectedUnit == null)
        {
            return;
        }

        Vector2 worldPoint = GetMouseWorldPoint();
        selectedUnit.SetDestination(worldPoint);
    }

    private void SelectUnit(UnitController unit)
    {
        if (selectedUnit == unit)
        {
            return;
        }

        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(false);
        }

        selectedUnit = unit;

        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(true);
        }

        SelectedUnitChanged?.Invoke(selectedUnit);
    }

    private UnitController GetSelectableUnitAt(Vector2 worldPoint)
    {
        int hitCount = Physics2D.OverlapPoint(worldPoint, selectionFilter, selectionHits);
        UnitController fallbackUnit = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = selectionHits[i];
            selectionHits[i] = null;

            if (hit == null)
            {
                continue;
            }

            UnitSelectionTarget selectionTarget = hit.GetComponent<UnitSelectionTarget>();
            if (selectionTarget != null)
            {
                return selectionTarget.Unit;
            }

            fallbackUnit ??= hit.GetComponentInParent<UnitController>();
        }

        return fallbackUnit;
    }

    private Vector2 GetMouseWorldPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -sceneCamera.transform.position.z;
        Vector3 worldPoint = sceneCamera.ScreenToWorldPoint(mousePosition);
        return worldPoint;
    }
}
