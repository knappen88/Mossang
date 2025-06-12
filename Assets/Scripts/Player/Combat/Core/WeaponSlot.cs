namespace Combat.Core
{
    using UnityEngine;

    [System.Serializable]
    public class WeaponSlot
    {
        public string slotName = "RightHand";
        public Transform slotTransform;
        public Vector3 localPosition;
        public Vector3 localRotation;
        public Vector3 localScale = Vector3.one;
    }
}
