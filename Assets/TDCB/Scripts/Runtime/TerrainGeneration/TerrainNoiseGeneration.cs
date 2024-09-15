using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDCB
{
    
    [CreateAssetMenu(menuName="TDCB/Terrain Feature")]
    public class TerrainNoiseGeneration : ScriptableObject
    {
        public enum Channel {Red, Blue, Green, Alpha}
        
        [SerializeField] private int width = 1024;
        [SerializeField] private int height = 1024;
        
        [SerializeField] private Texture2D texture;
        [SerializeField] private Channel channel;
        
        [SerializeField] private float scale = 20f;
        [SerializeField] private int octaves = 4;
        [SerializeField] private float persistance = 0.5f;
        [SerializeField] private float lacunarity = 2;
        [SerializeField] private float threshold = 0.5f;
        [SerializeField] private float centerClearRadius = 70;

        [SerializeField] private bool distanceAffectsThreshold;
        
        [SerializeField] private float offsetMulti = 9999f;
        [Button]
        public void RandomiseOffset()
        {
            offsetMulti = Random.Range(9000, 9999);
        }
        
        [HorizontalGroup]
        [SerializeField] private int seed;
        [HorizontalGroup]
        [Button]
        public void NewSeed()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        private Vector2 _centerPos;
        private Vector2 offset;


        [Button]
        public void Generate()
        {
            var noise = GenerateNoise();
            _centerPos = new Vector2(width / 2, height / 2);
            
            SetTexture(noise);
        }

        [Button]
        public void ClearTexture()
        {
            if (texture.width != width)
            {
                texture.Reinitialize(width, height);
            }
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, Color.black);
                }
            }
            texture.Apply();
            
#if UNITY_EDITOR
            WriteToTexture(texture);

            AssetDatabase.SaveAssets();
#endif
        }

        public float[,] GenerateNoise()
        {
            Random.InitState(seed);
            offset = new Vector2(Random.value*offsetMulti, Random.value*offsetMulti);

            return Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);
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
                    texture.SetPixel(x, y, GenerateColor(x, y,noise[x, y], Vector2.Distance(_centerPos, new Vector2(x,y))));
                }
            }
            
            texture.Apply();
            
#if UNITY_EDITOR
            WriteToTexture(texture);

            AssetDatabase.SaveAssets();
#endif
        }
        
        private Color GenerateColor(int x, int y, float sample, float distance)
        {
            bool closeToCenter = distance < centerClearRadius;

            if (distanceAffectsThreshold)
            {
                float minDistance = centerClearRadius;
                float maxDistance = width / 2f;
                sample *= Mathf.Lerp(threshold, 1, Mathf.InverseLerp(minDistance, maxDistance, distance));
            }
            bool fill = !closeToCenter && sample > threshold;
            return GetColor(x, y, fill);
        }

        private Color GetColor(int x, int y, bool filled)
        {
            Color current = texture.GetPixel(x, y);
            switch (channel)
            {
                case Channel.Red:
                    current.r = filled ? 1 : 0;
                    break;
                case Channel.Green:
                    current.g = filled ? 1 : 0;
                    break;
                case Channel.Blue:
                    current.b = filled ? 1 : 0;
                    break;
                case Channel.Alpha:
                    current.a = filled ? 1 : 0;
                    break;
            }

            return current;
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
    }
}
