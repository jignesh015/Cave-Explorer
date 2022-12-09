using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CaveExplorer
{
    public class EnvironmentController : MonoBehaviour
    {
        [Header("ENVIRONMENTS")]
        [SerializeField] private string envNamePrefix;
        public Environment lobbyEnv;
        public List<Environment> player1CaveEnvList;
        public List<Environment> player2CaveEnvList;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Loads the given environment
        /// </summary>
        /// <param name="_env"></param>
        /// <param name="_callback"></param>
        public void LoadEnvironment(Environment _env, UnityAction _callback = null)
        {
            StartCoroutine(LoadEnvironmentAsync(_env, _callback));
        }

        private IEnumerator LoadEnvironmentAsync(Environment _env, UnityAction _callback = null)
        {
            //Load Environment from Resource
            ResourceRequest _req = Resources.LoadAsync<GameObject>(envNamePrefix +_env.name);
            while(!_req.isDone)
            {
                yield return null;
            }
            GameObject _environmentReqObj = _req.asset as GameObject;

            //Instantiate and position environment
            GameObject _environmentObj = Instantiate(_environmentReqObj, transform);
            _environmentObj.name= _env.name;
            _environmentObj.transform.position = _env.environmentSpawnPos;
            _environmentObj.transform.rotation = _env.environmentSpawnRot;

            //Adjust fog density for the new environment
            RenderSettings.fogDensity = _env.fogDensity;

            //Invoke callback once environment is done loading
            _callback?.Invoke();
        }

        /// <summary>
        /// Destroys the given environment
        /// </summary>
        /// <param name="_env"></param>
        /// <param name="_delay"></param>
        public void UnloadEnvironment(Environment _env, float _delay = 0)
        {
            GameObject _envObj = GameObject.Find(_env.name);
            if(_envObj != null )
            {
                Debug.LogFormat("<color=red>Unloading {0}</color>", _envObj.name);
                Destroy(_envObj, _delay);
            }
        }
    }

    [Serializable]
    public class Environment
    {
        public string name;
        public Vector3 environmentSpawnPos;
        public Quaternion environmentSpawnRot;
        public Vector3 playerSpawnPos;
        public Quaternion playerSpawnRot;
        public float fogDensity;
    }
}
