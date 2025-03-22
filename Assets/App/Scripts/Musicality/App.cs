using System;
using UnityEngine;

namespace Musicality
{
    public class App : MonoBehaviour
    {
        [SerializeField] private ListenerIEM ListenerIem;
        public static ListenerIEM ListenerIEM;

        private void Awake()
        {
            ListenerIEM = ListenerIem;
        }
    }
}