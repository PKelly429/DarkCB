using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;
using UnityEngine.Pool;

namespace TDCB
{
    public class BindableIconUI : MonoBehaviour
    {
        public GameObject iconUI;
        private ObjectPool<AbstractBinder> iconPool;

        private void Awake()
        {
            iconPool = new ObjectPool<AbstractBinder>(CreateIcon, Get, Release, DestroyIcon);
        }

        public AbstractBinder GetIcon()
        {
            return iconPool.Get();
        }
        
        public void ReleaseIcon(AbstractBinder icon)
        {
            iconPool.Release(icon);
        }

        private void DestroyIcon(AbstractBinder obj)
        {
            Destroy(obj);
        }

        private void Release(AbstractBinder obj)
        {
            obj.Unbind();
            obj.gameObject.SetActive(false);
        }

        private void Get(AbstractBinder obj)
        {
            obj.gameObject.SetActive(true);
        }

        private AbstractBinder CreateIcon()
        {
            return Instantiate(iconUI, transform).GetComponent<AbstractBinder>();
        }
    }
}
