using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDCB
{
    [System.Serializable]
    public class TerrainGeneration : MonoBehaviour
    {
        [SerializeField] private Texture2D terrainFeaturesTexture;
        [SerializeField] private Texture2D terrainResourcesTexture;
        [SerializeField] private Texture2D pathfindingTexture;
        [SerializeField] private Texture2D validTerrainTexture;
        [SerializeField] private Terrain terrain;
        [SerializeField] private Terrain gridTerrain;
        [SerializeField] private int width = 1024;
        [SerializeField] private int height = 1024;
        [SerializeField] private float centerClearRadius = 70;

        [HorizontalGroup]
        [SerializeField] private int seed;
        [HorizontalGroup]
        [Button]
        public void NewSeed()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        [Header ("Noise")]
        [SerializeField] private float scale = 20f;
        [SerializeField] private int octaves = 4;
        [SerializeField] private float persistance = 0.5f;
        [SerializeField] private float lacunarity = 2;

        [Header("Terrain")] 
        [SerializeField] private int[] grassIds;
        [SerializeField] private int[] mudIds;
        [SerializeField] private int[] stoneIds;
        [SerializeField] private int[] rootIds;

        [Header("HeightMap")] 
        public float minGroundHeight = 0f;
        public float maxGroundHeight = 1f;

        [Header("Trees")] 
        [SerializeField] private int treeVariations = 7;
        [SerializeField] private float treeRadius = 1.5f;
        [SerializeField] private float treeThreshold = 0.4f;

        [Header("Rocks")] 
        [SerializeField] private int rockVariations = 4;
        [SerializeField] private float rockRadius = 0.5f;
        public int Width => width;
        public int Height => height;

        private Vector2 _centerPos;
        private Vector2 offset;
        private const float OffsetMulti = 9999f;

        private void Awake()
        {
            UpdateTextures();
        }

        public float[,] GenerateNoise()
        {
            offset = new Vector2(Random.value*OffsetMulti, Random.value*OffsetMulti);
        
            return Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);
        }

        [Button]
        public void Generate()
        {
            var terrainData = terrain.terrainData;
            terrainData.treeInstances = Array.Empty<TreeInstance>();
            
            Random.InitState(seed);
            var noise1 = GenerateNoise();
            var noise2 = GenerateNoise();
            var noise3 = GenerateNoise();
            var noise4 = GenerateNoise();
            var noise5 = GenerateNoise();
            
            UpdateTextures();
            
            
            // HEIGHT
            float[,] heightData = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    int heightX = (int)(((float)x / terrainData.heightmapResolution) * width);
                    int heightY = (int)(((float)y / terrainData.heightmapResolution) * height);

                    float heightValue = Mathf.Lerp(minGroundHeight, maxGroundHeight, noise1[heightX, heightY]);
                    if (terrainFeaturesTexture.GetPixel(heightX, heightY).b > 0.5f)
                    {
                        heightValue -= minGroundHeight;
                    }
                    heightData[y, x] = heightValue;
                }
            }
            
            terrainData.SetHeights(0, 0, heightData);
            gridTerrain.terrainData.SetHeights(0, 0, heightData);
            

            // TEXTURE
            float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    int splatX = (int)(((float)x / terrainData.alphamapWidth) * width);
                    int splatY = (int)(((float)y / terrainData.alphamapHeight) * height);
                    splatmapData[x, y, 0] = 1;

                    Color featureSample = terrainFeaturesTexture.GetPixel(splatX, splatY);
                    Color resourceSample = terrainResourcesTexture.GetPixel(splatX, splatY);
                    
                    splatmapData[y, x, 0] = 0.65f;
                    splatmapData[y, x, 1] = noise1[splatX, splatY] * Random.Range(0f, 1f) * 0.5f;
                    splatmapData[y, x, 2] = noise1[splatX, splatY] * Random.Range(0f, 1f) * 0.5f;
                    
                    splatmapData[y, x, 10] = noise2[splatX, splatY] * Random.Range(0f, 1f) * 0.75f;
                    splatmapData[y, x, 11] = noise3[splatX, splatY] * Random.Range(0f, 1f) * 0.5f;
                    splatmapData[y, x, 12] = noise4[splatX, splatY] * Random.Range(0f, 1f) * 0.5f;
                    splatmapData[y, x, 13] = noise5[splatX, splatY] * Random.Range(0f, 1f) * 0.25f;
                    
                    float stoneValue = resourceSample.g;
                    for(int i=0; i<stoneIds.Length; i++)
                    {
                        splatmapData[y, x, stoneIds[i]] = stoneValue;
                    }
                    
                    if (featureSample.b > 0.5f || Vector2.Distance(new Vector2(splatX, splatY), _centerPos) < centerClearRadius)
                    {
                        foreach (var rootId in rootIds)
                        {
                            splatmapData[y, x, rootId] = 0;
                        }
                        continue;
                    }
                    
                    float treeValue = featureSample.g;
                    for(int i=0; i<rootIds.Length; i++)
                    {
                        splatmapData[y, x, rootIds[i]] = treeValue;
                    }
                }
            }
            terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
            
            // TREES
            var treePositions = PoissonDiscSampling.GeneratePoints(treeRadius, new Vector2(width, height));
            foreach (var treePosition in treePositions)
            {
                if(Vector2.Distance(treePosition, _centerPos) < centerClearRadius) continue;
                Color sample = terrainFeaturesTexture.GetPixel((int)treePosition.x, (int)treePosition.y);
                if (sample.b <= 0.5f && sample.g > treeThreshold)
                {
                    TreeInstance newTree = new TreeInstance();
                    newTree.position = new Vector3(treePosition.x/width, 0, treePosition.y/height);
                    newTree.prototypeIndex = Random.Range(0,treeVariations);
                    newTree.heightScale = Random.Range(0.7f, 1.3f);
                    newTree.widthScale = Random.Range(0.9f, 1.1f);
                    newTree.color = Color.white;
                    newTree.lightmapColor = Color.white;
                    terrain.AddTreeInstance(newTree);
                }
            }
            
            // ROCKS
            var rockPositions = PoissonDiscSampling.GeneratePoints(rockRadius, new Vector2(width, height));
            foreach (var rockPosition in rockPositions)
            {
                Color sample = terrainResourcesTexture.GetPixel((int)rockPosition.x, (int)rockPosition.y);
                if (sample.g > 0.5f)
                {
                    TreeInstance newTree = new TreeInstance();
                    newTree.position = new Vector3(rockPosition.x/width, 0, rockPosition.y/height);
                    newTree.rotation = Random.Range(0, 360*Mathf.Deg2Rad);
                    newTree.prototypeIndex = Random.Range(treeVariations,treeVariations+rockVariations);
                    newTree.heightScale = Random.Range(0.7f, 1.3f);
                    newTree.widthScale = Random.Range(0.9f, 1.1f);
                    newTree.color = Color.white;
                    newTree.lightmapColor = Color.white;
                    terrain.AddTreeInstance(newTree);
                }
            }
            
            terrain.Flush();
            gridTerrain.Flush();
        }
        
        [Conditional("UNITY_EDITOR")]
        private void UpdateTextures()
        {
            if (terrainFeaturesTexture.width != width)
            {
                Debug.LogError("Texture size does not match");
                return;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color terrainTexSample = terrainFeaturesTexture.GetPixel(x, y);
                    Color resourceTexSample = terrainResourcesTexture.GetPixel(x, y);
                    bool valid = terrainTexSample.g < 0.5f && terrainTexSample.b < 0.5f && resourceTexSample.g < 0.5f;
                    pathfindingTexture.SetPixel(x, y, valid ? Color.green : Color.black);
                }
            }

            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    Color features = DownSample(terrainFeaturesTexture, x*4, y*4, 4);
                    Color resources = DownSample(terrainResourcesTexture, x*4, y*4, 4);

                    bool water = features.b >= 0.5f;
                    bool tree = features.g >= 0.5f;
                    bool stone = resources.g >= 0.5f;

                    bool valid = !water && !tree && !stone;

                    Color colour = valid ? Color.black : Color.red;
                    ResourceType resourceTypeInCell = ResourceType.None;
                    if (tree)
                    {
                        resourceTypeInCell = ResourceType.Wood;
                    }
                    else if (stone)
                    {
                        resourceTypeInCell = ResourceType.Stone;
                    }

                    colour.g = resourceTypeInCell.GetResourceTexMapColour() / (float) byte.MaxValue;
                    
                    validTerrainTexture.SetPixel(x, y, colour);
                }
            }
            
            validTerrainTexture.Apply();
            pathfindingTexture.Apply();
            
#if UNITY_EDITOR
            WriteToTexture(validTerrainTexture);
            WriteToTexture(pathfindingTexture);
            AssetDatabase.SaveAssets();
#endif
        }

        private Color DownSample(Texture2D tex, int x, int y, int scale)
        {
            Vector4 color = new Vector4();
            for (int z = 0; z < scale; z++)
            {
                for (int w = 0; w < scale; w++)
                {
                    var sample = tex.GetPixel(x + z, y + w);
                    color += new Vector4(sample.r, sample.g, sample.b, sample.a);
                }
            }

            color /= (scale * scale);

            return new Color(color.x, color.y, color.z, color.w);
        }
        
#if UNITY_EDITOR
        private void WriteToTexture(Texture2D tex)
        {
            if (Application.isPlaying)
            {
                return;
            }
            
            string path = Application.dataPath + "/../" + AssetDatabase.GetAssetPath(tex);
            System.IO.File.WriteAllBytes(path, tex.EncodeToJPG());
            
            EditorUtility.SetDirty(tex);
        }
#endif

        private Color GenerateColor(float sample, bool closeToCenter)
        {
            bool isTree = !closeToCenter && sample > treeThreshold;
            return isTree ? new Color(0, 0, 0, 1) : new Color(0, 1, 0, 1);
        }
    }
}
