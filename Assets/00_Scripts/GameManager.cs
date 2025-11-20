using System;
using Unity.VisualScripting;
using UnityEngine;

namespace _00_Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            MusicManager.Instance.PlayGameplayMusic();
        }
    }
}