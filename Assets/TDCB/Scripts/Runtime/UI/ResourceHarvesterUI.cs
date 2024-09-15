using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;
using UnityEngine.Pool;

namespace TDCB
{
    public class ResourceHarvesterUI : MonoBehaviour
    {
        public GameObject iconUI;
        private ObjectPool<AbstractBinder> iconPool = new ObjectPool<AbstractBinder>(CreateIcon, Get, Release, DestroyIcon);

        private void Awake()
        {
            iconUIPrefab = iconUI;
            parent = transform;
        }

        public AbstractBinder GetResourceHarvesterIcon()
        {
            return iconPool.Get();
        }
        
        public void ReleaseResourceHarvesterIcon(AbstractBinder icon)
        {
            iconPool.Release(icon);
        }

        private static GameObject iconUIPrefab;
        private static Transform parent;
        private static void DestroyIcon(AbstractBinder obj)
        {
            Destroy(obj);
        }

        private static void Release(AbstractBinder obj)
        {
            obj.Unbind();
            obj.gameObject.SetActive(false);
        }

        private static void Get(AbstractBinder obj)
        {
            obj.gameObject.SetActive(true);
        }

        private static AbstractBinder CreateIcon()
        {
            return Instantiate(iconUIPrefab, parent).GetComponent<AbstractBinder>();
        }
    }
}
