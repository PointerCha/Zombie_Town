using UnityEngine;

[DisallowMultipleComponent]
public class GeneratedTestUnitGizmos : MonoBehaviour
{
    [SerializeField] private Color selectedColor = new Color(0f, 1f, 0.25f, 1f);
    [SerializeField] private Color idleColor = new Color(0.1f, 0.7f, 1f, 1f);
    [SerializeField] private float radius = 0.35f;

    private GeneratedTestUnitController unit;

    private void OnDrawGizmos()
    {
        if (unit == null)
        {
            unit = GetComponent<GeneratedTestUnitController>();
        }

        Gizmos.color = unit != null && unit.IsSelected ? selectedColor : idleColor;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawLine(transform.position + Vector3.left * radius, transform.position + Vector3.right * radius);
        Gizmos.DrawLine(transform.position + Vector3.down * radius, transform.position + Vector3.up * radius);
    }
}
