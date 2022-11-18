using UnityEngine;

public class SpeciesSettings : MonoBehaviour
{
    [SerializeField] public Color cellColor;
    [SerializeField] public float velocity;
    [SerializeField] public float velocityVariation;
    [SerializeField] public float turnSpeed = 10f;
    [SerializeField] public float senseAngle = 1f;
    [SerializeField] public float senseDistance = 10f;

}