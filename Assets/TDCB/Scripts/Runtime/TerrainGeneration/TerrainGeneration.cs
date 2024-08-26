using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDCB
{
    [System.Serializable]
    public class TerrainGeneration : MonoBehaviour
    {
        [SerializeField] private Texture2D texture;
        [SerializeField] private Texture2D validTerrainTextire;
        [SerializeField] private Terrain terrain;
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

        [Header("Trees")] 
        [SerializeField] private int treeVariations = 7;
        [SerializeField] private float treeRadius = 1.5f;
        [SerializeField] private float treeThreshold = 0.4f;

        public int Width => width;
        public int Height => height;
        public float TreeThreshold => treeThreshold;

        private Vector2 _centerPos;
        private Vector2 offset;
        private const float OffsetMulti = 9999f;

        public void Awake()
        {
            GenerateTrees();
        }

        public float[,] GenerateNoise()
        {
            Random.InitState(seed);
            offset = new Vector2(Random.value*OffsetMulti, Random.value*OffsetMulti);

            return Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);
        }

        [Button]
        public void GenerateTrees()
        {
            var terrainData = terrain.terrainData;
            terrainData.treeInstances = Array.Empty<TreeInstance>();

            var noise = GenerateNoise();
            _centerPos = new Vector2(width / 2, height / 2);
            
            SetTexture(noise);
            
            // TEXTURE
            float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    int splatX = (int)(((float)x / terrainData.alphamapWidth) * width);
                    int splatY = (int)(((float)y / terrainData.alphamapHeight) * height);
                    splatmapData[x, y, 0] = 1;

                    if (Vector2.Distance(new Vector2(splatX, splatY), _centerPos) < centerClearRadius)
                    {
                        splatmapData[y, x, 21] = 0;
                        continue;
                    }
                    float noiseValue = noise[splatX, splatY];
                    if (noiseValue < treeThreshold) noiseValue = 0;
                    splatmapData[y, x, 21] = noiseValue;
                }
            }
            terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
            
            // TREES
            var treePositions = PoissonDiscSampling.GeneratePoints(treeRadius, new Vector2(width, height));
            foreach (var treePosition in treePositions)
            {
                if(Vector2.Distance(treePosition, _centerPos) < centerClearRadius) continue;
                if (noise[(int)treePosition.x, (int)treePosition.y] > treeThreshold)
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
            
            terrain.Flush();
        }
        
        [Conditional("UNITY_EDITOR")]
        private void SetTexture(float[,] noise)
        {
            if (texture.width != width)
            {
                texture.Reinitialize(width, height);
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, GenerateColor(noise[x, y], Vector2.Distance(_centerPos, new Vector2(x,y)) < centerClearRadius));
                }
            }
            
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    float av = texture.GetPixel(x * 2, y * 2).g;
                    av += texture.GetPixel((x * 2)+1, y * 2).g;
                    av += texture.GetPixel(x * 2, (y * 2)+1).g;
                    av += texture.GetPixel((x * 2)+1, (y * 2)+1).g;

                    bool valid = av > 2.5f;
                    
                    // bool valid = texture.GetPixel(x * 2, y * 2).g > 0.5f;
                    // valid &= texture.GetPixel((x * 2)+1, y * 2).g > 0.5f;
                    // valid &= texture.GetPixel(x * 2, (y * 2)+1).g > 0.5f;
                    // valid &= texture.GetPixel((x * 2)+1, (y * 2)+1).g > 0.5f;
                    
                    validTerrainTextire.SetPixel(x, y, valid ? Color.black : Color.red);
                }
            }
            
            texture.Apply();
            validTerrainTextire.Apply();
            
#if UNITY_EDITOR
            WriteToTexture(texture);
            WriteToTexture(validTerrainTextire);
            AssetDatabase.SaveAssets();
#endif
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
