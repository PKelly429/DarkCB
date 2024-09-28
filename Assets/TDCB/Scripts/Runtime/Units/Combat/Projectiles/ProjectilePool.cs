using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace TDCB
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;

        private ObjectPool<Projectile> projectiles;

        private void Awake()
        {
            projectiles = new ObjectPool<Projectile>(CreateProjectile, GetProjectile, ReleaseProjectile, DestroyProjectile);
        }

        public void ShootProjectile(Vector3 startPos, Transform target, float speed, Action onHit)
        {
            var projectile = projectiles.Get();
            onHit += () => projectiles.Release(projectile);
            projectile.Shoot(startPos, target, speed, onHit);
        }

        #region Pool
        private void DestroyProjectile(Projectile obj)
        {
            Destroy(obj);
        }

        private void ReleaseProjectile(Projectile obj)
        {
            obj.SetActive(false);
        }

        private void GetProjectile(Projectile obj)
        {
            obj.SetActive(true);
        }

        private Projectile CreateProjectile()
        {
            return Instantiate(projectilePrefab).GetComponent<Projectile>();
        }
        #endregion
    }
}
