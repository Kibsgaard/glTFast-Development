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
            var importSettings = new ImportSettings();
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

            Debug.Log($"[{name}] Instantiated main scene");
            
            if (_logVertsAndIndices)
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    Debug.Log(string.Join(", ", meshFilter.sharedMesh.vertices));
                    Debug.Log(string.Join(", ", meshFilter.sharedMesh.triangles));
                }
                var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    Debug.Log(string.Join(", ", skinnedMeshRenderer.sharedMesh.vertices));
                    Debug.Log(string.Join(", ", skinnedMeshRenderer.sharedMesh.triangles));
                }
            }
        }
    }
}