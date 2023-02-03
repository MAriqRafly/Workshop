using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class NetworkHelper : MonoBehaviour, INetworkRunnerCallbacks
{

    public NetworkPrefabRef PlayerData;
    public MenuController menuController;
    public DataManager dataManager;
    public NetworkPrefabRef PlayerChar;
    public NetworkPrefabRef Prefab_DataManager;
    #region Used Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject NO;
        if(runner.LocalPlayer == player)
        {
            NO = runner.Spawn(PlayerData , inputAuthority: player);
            NO.GetComponent<PlayerData>().PlayerName = PlayerPrefs.GetString("PlayerName");
            NO.GetComponent<PlayerData>().AvatarIndex = PlayerPrefs.GetInt("Avatar");
            runner.SetPlayerObject(player,NO);
            if(FindObjectOfType<MenuController>() != null)
            {
                menuController = FindObjectOfType<MenuController>();
                menuController.JoinLobby(true) ;
                menuController.Runner = runner;

            }

            //playerChar.transform.position = FindObjectOfType<PlayerSpawnerPrototype>().transform.position;
        }

        menuController.UpdatePlayerList();

    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if(SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (runner.IsSharedModeMasterClient)
            {
                //runner.Spawn(Prefab_DataManager, inputAuthority: runner.LocalPlayer);
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (FindObjectOfType<DataManager>() != null)
        {
            FindObjectOfType<DataManager>().RemovePlayer(player);
        }

        if(FindObjectOfType<MenuController>() != null)
        {
            FindObjectOfType<MenuController>().UpdatePlayerList();

        }
    }
    #endregion
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene(0);
    }

    #region Unused Callbacks


    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }


    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
