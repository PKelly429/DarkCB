using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace TDCB
{
    public class CommandFeedbackSpawner : MonoBehaviour, ICommandListener<Vector3>
    {
        [SerializeField] private MoveCommand command;
        [SerializeField] private GameObject feedbackPrefab;
        
        private ObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(CreatePooledObj, GetPooledObj, ReleasePooledObj, DestroyPooledObj);
        
        private void OnEnable()
        {
            PooledPrefab = feedbackPrefab;
            command.Register(this);
        }

        private void OnDisable()
        {
            command.Deregister(this);
        }

        public void Raise(Vector3 position)
        {
            position += new Vector3(0, 0.1f, 0);
            var obj = objectPool.Get();
            obj.transform.position = position;
            Tween.Delay(1f, () => { objectPool.Release(obj); });
        }
        
        
        #region UnitHighlightPool

        private static GameObject PooledPrefab;
        private static void DestroyPooledObj(GameObject obj)
        {
            Destroy(obj);
        }

        private static void ReleasePooledObj(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void GetPooledObj(GameObject obj)
        {
            obj.SetActive(true);
        }

        private static GameObject CreatePooledObj()
        {
            return Instantiate(PooledPrefab);
        }
        #endregion
    }
}
