using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

namespace App.Scripts
{
    public class AppNetworkConnection : MonoBehaviour
    {
        /// <summary>
        /// Found NetworkManager.
        /// </summary>
        private NetworkManager _networkManager;
        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        /// <summary>
        /// Current state of server socket.
        /// </summary>
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;

        private enum Role
        {
            Client,
            Server,
        }

        [SerializeField] private Role appRole = Role.Client;
        
        private void Start()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, HUD will not function.");
                return;
            }
            else
            {
                // UpdateColor(LocalConnectionState.Stopped, ref _serverIndicator);
                // UpdateColor(LocalConnectionState.Stopped, ref _clientIndicator);
                _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            }
            
            switch (appRole)
            {
                case Role.Server:
                        _networkManager.ServerManager.StartConnection();
                    break;
                case Role.Client:
                        _networkManager.ClientManager.StartConnection();
                    break;
            }
        }
        
        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
            // UpdateColor(obj.ConnectionState, ref _clientIndicator);
        }


        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
            // UpdateColor(obj.ConnectionState, ref _serverIndicator);
        }
        
        private void OnDestroy()
        {
            if (_networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }
    }
}