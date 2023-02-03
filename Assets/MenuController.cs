using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_InputField IF_Name;
    public TMP_InputField IF_RoomName;

    public Button Button_NextAva;
    public Button Button_PrevAva;
    public Button Button_Join;
    public GameObject PlayCam;
    public GameObject MainCam;
    [Header("Avatar")]
    public Transform Avatars;
    public int AvatarIndex = 0; 
    public GameObject LobbyPanel;
    public GameObject MainMenuPanel;
    

    [Header("Multipalyer")]
    public GameObject Prefab_NetworkRunner;
    public NetworkRunner Runner;
    public Button Button_Start;
    public Button Button_Leave;
    public string NextRoomName;
    public TMP_Text RoomName;
 

    public TMP_Text TMP_PlayerList;
    void Start()
    {
        Button_NextAva.onClick.AddListener(NextAva);
        Button_PrevAva.onClick.AddListener(PrevAva);
        Button_Join.onClick.AddListener(CreateOrJoinWaitingRoom);
        Button_Start.onClick.AddListener(StartRoom);
        Button_Leave.onClick.AddListener(LeaveRoom);
    }

    // Update is called once per frame
    public void NextAva()
    {
        AvatarIndex++;
        if (AvatarIndex >= Avatars.childCount)
            AvatarIndex = 0;

        for(int i = 0; i < Avatars.childCount; i++)
        {
            if(i == AvatarIndex)
                Avatars.GetChild(i).gameObject.SetActive(true);
            else
                Avatars.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void PrevAva()
    {
        AvatarIndex--;
        if (AvatarIndex < 0)
            AvatarIndex = Avatars.childCount-1;

        for (int i = 0; i < Avatars.childCount; i++)
        {
            if (i == AvatarIndex)
                Avatars.GetChild(i).gameObject.SetActive(true);
            else
                Avatars.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void LeaveRoom()
    {
        Runner.Shutdown(destroyGameObject: true);
    }

    public void CreateOrJoinWaitingRoom()
    {
        if (Runner == null)
        {
            GameObject NR = GameObject.Instantiate(Prefab_NetworkRunner);
            Runner = NR.GetComponent<NetworkRunner>();
            Runner.ProvideInput = true;
        }
        PlayerPrefs.SetString("PlayerName", IF_Name.text);
        PlayerPrefs.SetInt("Avatar", AvatarIndex);
        CreateRoom();
    }
    public async void CreateRoom()
    {
        string roomName = IF_RoomName.text;
        if (roomName == "")
        {
            return;
        }

        var result = await Runner.StartGame(new StartGameArgs()
        {

            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>()
        });
    }


    public void StartPlayCam()
    {
        PlayCam.SetActive(true);
        LobbyPanel.SetActive(false);
        MainCam.SetActive(false);
    }
    public void JoinLobby(bool condition)
    {
        MainMenuPanel.SetActive(!condition);
        LobbyPanel.SetActive(condition);
    }

    Coroutine cr_updatePlayerList;
    public void UpdatePlayerList()
    {
        RoomName.text = $"Room : {Runner.SessionInfo.Name}";
        if (cr_updatePlayerList != null)
            StopCoroutine(cr_updatePlayerList);
        cr_updatePlayerList = StartCoroutine(CR_UpdatePlayerList());
    }
    public IEnumerator CR_UpdatePlayerList()
    {
        string playerListText = "";
        foreach(PlayerRef player in Runner.ActivePlayers)
        {
            while(Runner.GetPlayerObject(player) == null)
            {
                yield return null;
            }
            PlayerData pd = Runner.GetPlayerObject(player).GetComponent<PlayerData>();

            playerListText += $"{pd.PlayerName}";
            if (Runner.LocalPlayer == player)
            {
                playerListText += " (You)";
            }
            playerListText += "\n";
        }

        if (Runner.IsSharedModeMasterClient)
        {
            Button_Start.gameObject.SetActive(true);
        }
        else
        {
            Button_Start.gameObject.SetActive(false);

        }
        TMP_PlayerList.text = playerListText;
        yield return null;
    }

    public void StartRoom()
    {
        Runner.SessionInfo.IsOpen = false;
        Runner.SetActiveScene(NextRoomName);
    }

}
