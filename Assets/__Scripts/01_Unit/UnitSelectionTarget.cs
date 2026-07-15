using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class UnitSelectionTarget : MonoBehaviour
{
    [SerializeField] private UnitController unit;
    [SerializeField] private Collider2D selectionCollider;
    [SerializeField] private bool syncLayerWithUnit = true;

    public UnitController Unit => unit;
    public Collider2D SelectionCollider => selectionCollider;

    private void Awake()
    {
        if (unit == null)
        {
            unit = GetComponentInParent<UnitController>();
        }

        if (selectionCollider == null)
        {
            selectionCollider = GetComponent<Collider2D>();
        }

        if (selectionCollider != null)
        {
            selectionCollider.isTrigger = true;
        }

        SyncLayer();
    }

    private void OnValidate()
    {
        if (unit == null)
        {
            unit = GetComponentInParent<UnitController>();
        }

        if (selectionCollider == null)
        {
            selectionCollider = GetComponent<Collider2D>();
        }

        if (selectionCollider != null)
        {
            selectionCollider.isTrigger = true;
        }

        SyncLayer();
    }

    private void SyncLayer()
    {
        if (!syncLayerWithUnit || unit == null)
        {
            return;
        }

        gameObject.layer = unit.gameObject.layer;
    }
}
