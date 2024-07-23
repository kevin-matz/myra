using System;
using System.Collections;
using DG.Tweening;
using Showcase;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Simulation
{
    public class AnimationHandler : MonoBehaviour
    {
        public int distance = 50;
        public float time = 5.0f;
        public bool forwarden = false;
        private Animator _animator;
        private Vector3 _startPos;
        private Tween _tween;
        private int scene;
        private void Start()
        {
            _startPos = transform.position;
            _animator = gameObject.GetComponent<Animator>();
            if (_animator != null) _animator.enabled = false;
            SimulationManager.Instance.simulationToggle.AddListener(SimulationToggle);
            SimulationToggle();
        }

        private void SimulationToggle()
        {
            if(SimulationManager.Instance.simulation)
            {
                if (_animator != null) _animator.enabled = true;
                if (!forwarden) return;
                _startPos = transform.position;
                FastForward();
            }
            else
            {
                if (_animator != null) _animator.enabled = false;
                transform.position = _startPos;
                _tween?.Kill();
            }
        }

        private void FastForward()
        {
            transform.position = _startPos;
            scene = SceneManager.GetActiveScene().buildIndex;
            var isSpawnedOnPlayground = gameObject.CompareTag("SpawnedObject");
            if (isSpawnedOnPlayground && scene != 3)
            {
                _tween = transform.DOMove(_startPos + (distance * PlaygroundHandler.CurrentScale * transform.forward), time).OnComplete(FastForward);
            }
            else
            {
                _tween = transform.DOMove(_startPos + (distance * transform.forward), time).OnComplete(FastForward);
            }
        }

        private void FixedUpdate()
        {
            if (!SimulationManager.Instance.simulation)
                return;
            
            if (!Physics.Raycast(transform.position + transform.up * 100f, 
                    transform.up * -1, 
                    float.PositiveInfinity,
                    LayerMask.GetMask("MarkerTrackedGround"))
                && scene != 1 && scene != 3)
            {
                transform.position = _startPos;
                _tween?.Kill();
                SimulationToggle();
            }
        }
    }
}