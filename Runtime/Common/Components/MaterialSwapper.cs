using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class MaterialSwapper : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Material _newMaterial;

        private readonly Dictionary<Renderer, Material[]> _originalMaterialsMap = new();

        private void Start()
        {
            if (_renderers.IsNullOrEmpty()) { FetchRenderers(); }
        }

        [Button]
        public void FetchRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>();
        }

        [Button]
        public void SwapMaterials()
        {
            foreach (var renderer in _renderers)
            {
                var originalMaterials = Application.isPlaying ? renderer.materials : renderer.sharedMaterials;
                _originalMaterialsMap.TryAdd(renderer, originalMaterials);
                SetMaterials(renderer, originalMaterials.Select(m => _newMaterial).ToArray());
            }
        }

        [Button]
        public void RevertMaterials()
        {
            foreach (var (renderer, originalMaterials) in _originalMaterialsMap)
            {
                SetMaterials(renderer, originalMaterials);
            }
            _originalMaterialsMap.Clear();
        }

        private void SetMaterials(Renderer renderer, Material[] materials)
        {
            if (renderer == null) { return; }
            if (Application.isPlaying)
            {
                renderer.materials = materials;
            }
            else
            {
                renderer.sharedMaterials = materials;
            }
        }
    }
}