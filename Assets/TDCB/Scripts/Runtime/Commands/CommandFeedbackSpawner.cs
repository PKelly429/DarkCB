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

        private ObjectPool<GameObject> moveCommandPool;
        
        private void OnEnable()
        {
            moveCommandPool = new ObjectPool<GameObject>(CreateMoveObj, GetMoveObj, ReleaseMoveObj, DestroyMoveObj);
            command.Register(this);
        }

        private void OnDisable()
        {
            command.Deregister(this);
        }

        public void Raise(Vector3 position)
        {
            position += new Vector3(0, 0.1f, 0);
            var obj = moveCommandPool.Get();
            obj.transform.position = position;
            Tween.Delay(1f, () => { moveCommandPool.Release(obj); });
        }
        
        
        #region Pool
        private void DestroyMoveObj(GameObject obj)
        {
            Destroy(obj);
        }

        private void ReleaseMoveObj(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void GetMoveObj(GameObject obj)
        {
            obj.SetActive(true);
        }

        private GameObject CreateMoveObj()
        {
            return Instantiate(feedbackPrefab);
        }
        
        
        #endregion
    }
}
