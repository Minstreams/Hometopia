using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/MaterialUnitSetter")]
        public class MaterialUnitSetter : MonoBehaviour
        {
            public Material[] mats;
            [System.Serializable]
            public struct MaterialPair
            {
                public Renderer renderer;
                public Material[] origMats;

                public MaterialPair(Renderer renderer, Material[] origMats)
                {
                    this.renderer = renderer;
                    this.origMats = origMats;
                }
            }

            public List<MaterialPair> extraPairs;
            private List<MaterialPair> innerPairs;

            public int index;

            private void Awake()
            {
                Init();
            }
            public void Init()
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                innerPairs = new List<MaterialPair>(renderers.Length);
                foreach (Renderer r in renderers)
                {
                    innerPairs.Add(new MaterialPair(r, r.sharedMaterials));
                }
            }

            //Input
            [ContextMenu("Set")]
            public void Set()
            {
                Set(index);
            }
            public void Set(int index)
            {
                Set(mats[index]);
            }
            public void Set(Material mat, bool record = true)
            {
                for (int i = 0; i < innerPairs.Count; i++)
                {
                    Material[] ms = new Material[innerPairs[i].origMats.Length];
                    if (record) innerPairs[i] = new MaterialPair(innerPairs[i].renderer, innerPairs[i].renderer.sharedMaterials);
                    for (int j = ms.Length - 1; j >= 0; j--)
                    {
                        ms[j] = mat;
                    }
                    innerPairs[i].renderer.sharedMaterials = ms;
                }
                if (extraPairs != null)
                    for (int i = 0; i < extraPairs.Count; i++)
                    {
                        Material[] ms = new Material[extraPairs[i].origMats.Length];
                        if (record) extraPairs[i] = new MaterialPair(extraPairs[i].renderer, extraPairs[i].renderer.sharedMaterials);
                        for (int j = ms.Length - 1; j >= 0; j--)
                        {
                            ms[j] = mat;
                        }
                        extraPairs[i].renderer.sharedMaterials = ms;
                    }
            }

            public void ResetAll()
            {
                foreach (MaterialPair mp in innerPairs)
                {
                    if (mp.renderer != null)
                        mp.renderer.sharedMaterials = mp.origMats;
                }
                if (extraPairs != null)
                    foreach (MaterialPair mp in extraPairs)
                    {
                        if (mp.renderer != null)
                            mp.renderer.sharedMaterials = mp.origMats;
                    }
            }
        }
    }
}
