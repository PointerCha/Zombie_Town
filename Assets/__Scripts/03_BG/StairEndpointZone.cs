using UnityEngine;

[DisallowMultipleComponent]
public sealed class StairEndpointZone : MonoBehaviour
{
    [SerializeField] private StairFloorSwitchZone stairZone;

    public StairFloorSwitchZone StairZone => stairZone;

    public void Initialize(StairFloorSwitchZone owner)
    {
        if (stairZone == null)
        {
            stairZone = owner;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<UnitSelectionTarget>() != null)
        {
            return;
        }

        UnitFloorAgent unit = other.GetComponentInParent<UnitFloorAgent>();
        if (unit == null || stairZone == null)
        {
            return;
        }

        unit.TryEnterStairEndpoint(stairZone, this);
    }
}
