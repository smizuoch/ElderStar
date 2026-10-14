using UnityEngine;

namespace Nova.Runtime.UI
{
    [ExecuteAlways, DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class NovaCanvasLightMaterial : MonoBehaviour, UnityEngine.UI.IMaterialModifier
    {
        internal NovaCanvasLighting Owner { get; private set; }
        internal UnityEngine.UI.Graphic Graphic => graphic != null ? graphic : (graphic = GetComponent<UnityEngine.UI.Graphic>());
        private UnityEngine.UI.Graphic graphic;
        private Material drawingMaterial;

        internal void SetOwner(NovaCanvasLighting owner)
        {
            Owner = owner;
            if (owner == null) ReleaseMaterial();
            if (Graphic != null) Graphic.SetMaterialDirty();
        }
        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || Owner == null || !Owner.isActiveAndEnabled || baseMaterial == null || baseMaterial.shader.name != "Nova/UIParticles/UberLit")
            { ReleaseMaterial(); return baseMaterial; }
            if (drawingMaterial == null || drawingMaterial.shader != baseMaterial.shader)
            {
                ReleaseMaterial();
                drawingMaterial = new Material(baseMaterial) { name = baseMaterial.name + " (NOVA Canvas lighting)", hideFlags = HideFlags.HideAndDontSave };
            }
            else drawingMaterial.CopyPropertiesFromMaterial(baseMaterial);
            Owner.Apply(drawingMaterial, Graphic);
            return drawingMaterial;
        }
        internal void RefreshLighting() { if (drawingMaterial != null && Owner != null) Owner.Apply(drawingMaterial, Graphic); }
        private void ReleaseMaterial() { if (drawingMaterial != null) NovaCanvasLighting.Release(drawingMaterial); drawingMaterial = null; }
        private void OnDisable() { ReleaseMaterial(); if (Graphic != null) Graphic.SetMaterialDirty(); }
        private void OnDestroy() { ReleaseMaterial(); }
    }
}
