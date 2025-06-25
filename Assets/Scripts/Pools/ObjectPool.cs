//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Object Pool (Base)
//             Author: Christopher A
//             Date Created: 24th June, 2025
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//      Object Pool base class.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicUnity2DShooter
{
    public abstract class ObjectPool<T> : MonoBehaviour
        where T : UnityEngine.Object
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [SerializeField] protected T m_prefab = null!;
        [SerializeField] protected Transform m_parentTransform = null!;
        
        [SerializeField, Tooltip("Number of Objects to preallocate by default")]
        protected int m_preallocationCount = 50;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Non-Inspector Fields
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private readonly List<T> m_allPooledObjects = new List<T>();
        private readonly List<T> m_freeObjects = new List<T>();
        private readonly LinkedList<T> m_activeObjects = new LinkedList<T>();

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Properties
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        int ActiveObjectsCount => m_activeObjects.Count;
        int FreeObjectsCount => m_freeObjects.Count;
        int TotalObjectsCount => m_allPooledObjects.Count;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Unity Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        protected void Awake()
        {
            Initialise(m_preallocationCount);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //          Methods
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary> Acquires an inactive object from the pool. </summary>
        public T GetOrAddFreeObject()
        {
            if (m_freeObjects.Count == 0)
            {
                // Add a new one
                T spawnedObj = Instantiate(m_prefab, m_parentTransform);
                m_allPooledObjects.Add(spawnedObj);
                m_activeObjects.AddLast(spawnedObj);
                OnElementSpawned(spawnedObj);
                return spawnedObj;
            }

            // Get free obj. Fetching from back to avoid list element swapping overhead
            T obj = m_freeObjects[^1];
            m_freeObjects.RemoveAt(m_freeObjects.Count - 1);
            m_activeObjects.AddLast(obj);

            // The onus is now on the caller to let the pool know when the obj is free again.
            return obj;
        }

        /// <summary> Releases Object back into the pool. </summary>
        public void ReleaseObj(T _obj)
        {
            if (_obj == null)
            {
                return;
            }

            if (m_activeObjects.Remove(_obj))
            {
                m_freeObjects.Add(_obj);
            }
            else
            {
                Debug.LogError("Could not locate Obj to free up in pool. Maybe you tried to free it twice?");
            }
        }

        /// <summary> Pre-allocates the pool of objects. Instantiating up to the given pool size. </summary>
        protected void Initialise(int _poolSize)
        {
            if (m_allPooledObjects.Capacity < _poolSize)
                m_allPooledObjects.Capacity = _poolSize;

            if (m_allPooledObjects.Capacity < _poolSize)
                m_freeObjects.Capacity = _poolSize;

            for (int i = 0; i < _poolSize; ++i)
            {
                T spawnedObject = Instantiate(m_prefab, m_parentTransform);
                OnElementSpawned(spawnedObject);

                m_allPooledObjects.Add(spawnedObject);
                m_freeObjects.Add(spawnedObject);
            }
        }

        protected virtual void OnElementSpawned(T _obj)
        {
        }
    }
}
