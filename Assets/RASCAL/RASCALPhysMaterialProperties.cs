using UnityEngine;
using System.Collections;

public class RASCALPhysMaterialProperties : MonoBehaviour {

    [Tooltip("If checked, this material will override any other physics material settings.")]
    public bool overrideOthers = false;

    [Tooltip("The physics material to apply to the bone mesh colliders.")]
    public PhysicMaterial physicsMaterial;
}
