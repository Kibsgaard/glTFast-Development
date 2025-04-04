using System;
using System.IO;
using System.Linq;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;

namespace GLTFast.Development
{
    public class DracoSubmeshesLoad : MonoBehaviour
    {
        [SerializeField] private bool _logVertsAndIndices;
        [SerializeField] private bool _logBounds;
        
        private async void OnEnable()
        {
            // Cleanup as OnEnable will rerun after recompile and continue to play, reimporting the model.
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Destroy(transform.GetComponent<Animation>());
            await Awaitable.NextFrameAsync();

            var logger = new ConsoleLogger();
            var gltfImport = new GLTFast.GltfImport(logger: logger);
            var importSettings = new ImportSettings{NodeNameMethod = NameImportMethod.OriginalUnique};
            var success = await gltfImport.Load(Path.Combine(Application.streamingAssetsPath, $"{name}.glb"),
                importSettings);
            if (!success)
            {
                Debug.LogError($"[{name}] Failed to load gltf file");
                return;
            }
            
            var instantiationSettings = new InstantiationSettings();
            var gameObjectInstantiator = new GameObjectInstantiator(gltfImport, transform, logger: logger, instantiationSettings);
            success = await gltfImport.InstantiateMainSceneAsync(gameObjectInstantiator);
            if (!success)
            {
                Debug.LogError($"[{name}] Failed to instantiate main scene");
                return;
            }
            
            var animation = transform.GetComponentInChildren<Animation>();
            if (animation != null)
                animation.Play();

           
            if (_logVertsAndIndices)
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    Debug.Log(string.Join(", ", name, meshFilter.sharedMesh.vertices), this);
                    Debug.Log(string.Join(", ", name, meshFilter.sharedMesh.triangles), this);
                }
                var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    Debug.Log(string.Join(", ", name, skinnedMeshRenderer.sharedMesh.vertices), this);
                    Debug.Log(string.Join(", ", name, skinnedMeshRenderer.sharedMesh.triangles), this);
                }
            }

            if (_logBounds)
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    Debug.Log(string.Join(", ", name, meshFilter.sharedMesh.bounds), this);
                }
                var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    Debug.Log(string.Join(", ", name, skinnedMeshRenderer.sharedMesh.bounds), this);
                }
            }
        }
    }
}