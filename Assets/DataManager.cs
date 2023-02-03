using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class DataManager : NetworkBehaviour
{
    // Start is called before the first frame update

    [System.Serializable]
    public class PlayerObject
    {
        public NetworkObject playerChar;
        public string playerName;
        public int avatarIndex;
        public PlayerRef playerRef;
        public bool ready;
        public bool isDeath;
        public int deathCount = 0;
    }

    public List<PlayerObject> playerObjects;
    public Dictionary<PlayerRef, PlayerObject> D_playerObjects;
    public NetworkPrefabRef PlayerChar;
    public Transform SpawnPoint;
    #region Spawn
    public override void Spawned()
    {
        base.Spawned();
        Runner.AddSimulationBehaviour(Object.GetBehaviour<DataManager>() , Object);
        Debug.Log("DataManager Spawned");
        SpawnChar();
    }

    public void SpawnChar()
    {
        Vector3 SpawnPosition = SpawnPoint.position + new Vector3(Random.Range(-5, 5), 1, Random.Range(-1, 1));
        NetworkObject NO = Runner.Spawn(PlayerChar, SpawnPosition, inputAuthority: Runner.LocalPlayer );
        RPC_UpdateChar();
    }

    public void Awake()
    {
        playerObjects = new List<PlayerObject>();
        D_playerObjects = new Dictionary<PlayerRef, PlayerObject>();

    }

    Coroutine cr_UpdateChar;
    [Rpc]
    public void RPC_UpdateChar()
    {
        Debug.Log("RPC UPDATE CHAR");
        if (cr_UpdateChar != null)
            StopCoroutine(cr_UpdateChar);
        cr_UpdateChar = StartCoroutine(CR_UpdateChar());
    }

    public IEnumerator CR_UpdateChar()
    {

        PlayerBehaviour[] pbs =  FindObjectsOfType<PlayerBehaviour>();
        
        foreach (PlayerBehaviour pb in pbs)
        {
            while(Runner.GetPlayerObject(pb.Object.StateAuthority) == null)
            {
                yield return null;
            }

            PlayerData pd = Runner.GetPlayerObject(pb.Object.StateAuthority).GetComponent<PlayerData>();
            pb.UpdateChar(pd.AvatarIndex);
            Debug.Log(pb + "- avatar :  "+ pd.AvatarIndex.ToString());

            pb.GetComponentInChildren<TMP_Text>().text = pd.PlayerName;
            PlayerObject po = new PlayerObject();
            po.avatarIndex = pd.AvatarIndex;
            po.playerName = pd.PlayerName;
            po.playerRef = pd.Object.StateAuthority;
            po.playerChar = pb.Object;
            if(!D_playerObjects.ContainsKey(pb.Object.InputAuthority) )
                D_playerObjects.Add(pb.Object.InputAuthority, po);
            playerObjects.Add(po);
        }
        yield return null;
    }

    public void RemovePlayer(PlayerRef player)
    {
        D_playerObjects.Remove(player);
        if(Runner.IsSharedModeMasterClient)
            RPC_UpdatePlayerCount();
    }
    #endregion

    #region pre-game
    [Header("Pre-Game")]
    public GameObject Pre_Game;
    public TMP_Text TMP_PlayerCount;
    Coroutine updatePlayerCount;
    public int GameCounter = 0;
    [Header("Games")]
    public GameObject IPU;
    [Rpc]
    public void RPC_SetReady(PlayerRef player)
    {
        D_playerObjects[player].ready = true;
        if(Runner.IsSharedModeMasterClient)
            RPC_UpdatePlayerCount();
    }
    public void RPC_SetNotReady(PlayerRef player)
    {
        D_playerObjects[player].ready = false;
        if (Runner.IsSharedModeMasterClient)
            RPC_UpdatePlayerCount();
    }



    [Rpc]
    public void RPC_UpdatePlayerCount()
    {
        if(updatePlayerCount != null)
        {
            StopCoroutine(updatePlayerCount);
        }
        updatePlayerCount = StartCoroutine(CR_UpdatePlayerCount());
    }
    public IEnumerator CR_UpdatePlayerCount()
    {
        int playerCount = 0;
        int playerReady = 0;
        foreach(PlayerRef player in D_playerObjects.Keys)
        {
            if(D_playerObjects[player].ready == true)
            {
                playerReady++;
            }
            playerCount++;
        }

        TMP_PlayerCount.text = $"({playerReady}/{playerCount})";
        if(playerCount== playerReady)
        {
            if (Runner.IsSharedModeMasterClient)
            {
                RPC_StartGame();
            }
            
        }
        yield return null;
    }

    [Rpc]
    public void RPC_StartGame() 
    {
        Debug.Log("Game Starting");
        Pre_Game.SetActive(false);
        Round = 0;
        if (Runner.IsSharedModeMasterClient)
        {
            if(GameCounter == 0)
            {
                StartQuestion();
            }
        }

    }



    [Rpc]
    public void RPC_SetDeath(PlayerRef player)
    {
        D_playerObjects[player].isDeath = true;
        D_playerObjects[player].deathCount += 1;
    }

    [Rpc]
    [ContextMenu("Reset All Char")]
    public void RPC_Reset()
    {
        PlayerBehaviour[] pbs = FindObjectsOfType<PlayerBehaviour>();

        Cheering.Play();
        BGM_Fun.Play();
        BGM_Dramatic.Stop();
        
        IPU.SetActive(false);
        Pre_Game.SetActive(true);

        Floor.gameObject.SetActive(true);
        PlatformTrue.SetActive(false);
        PlatformFalse.SetActive(false);

        Round = 0;
        typingSpeed = 0.1f;
        foreach (PlayerRef player in D_playerObjects.Keys)
        {
            D_playerObjects[player].isDeath = false;
            D_playerObjects[player].ready = false;

        }

        foreach (PlayerBehaviour pb in pbs)
        {
            if(pb.Object.StateAuthority == Runner.LocalPlayer)
            {
                pb.nccp.enabled = false;
                pb.AllowMovement = true;
                Vector3 SpawnPosition = SpawnPoint.position + new Vector3(Random.Range(-5, 5), 1, Random.Range(-1, 1));
                pb.transform.position = SpawnPosition;
                pb.nccp.enabled = true;

            }
        }
        if (Runner.IsSharedModeMasterClient)
            RPC_UpdatePlayerCount();
    }
    #endregion

    #region True Or False Game

    [System.Serializable]
    public class Question
    {
        public string question_string;
        public bool answer;

    }

    public TMP_Text TMP_Time;
    public TMP_Text TMP_Question;
    public List<Question> QuestionPool;

    public GameObject Floor;
    public GameObject PlatformTrue;
    public GameObject PlatformFalse;

    public AudioSource BGM_Fun;
    public AudioSource BGM_Dramatic;
    public AudioSource Cheering;
    public int Round = 0;
    public float typingSpeed = 0.1f;
    public bool[] AskedQuestion;
    public void StartQuestion()
    {
        TMP_Question.text = "";
        if(Round == 0)
        {
            AskedQuestion = new bool[QuestionPool.Count];
        }
        int id = Random.Range(0, QuestionPool.Count);


        while (AskedQuestion[id] == true)
        {
            id = Random.Range(0, QuestionPool.Count);
        }
        AskedQuestion[id] = true;



        bool leftSide = Random.Range(0, 2) == 1;
        float x1 = 0;
        float z1 = 0;
        float x2 = 0;
        float z2 = 0;
        if (leftSide)
        {
             x1 = Random.Range(-6f, 0f);
             z1 = Random.Range(-5f, 3f);
             x2 = Random.Range(0f, 6f);
             z2 = Random.Range(-5f, 3f);
        }
        else
        {
             x1 = Random.Range(0, 6f);
             z1 = Random.Range(-5f, 3f);
             x2 = Random.Range(-6f, 0f);
             z2 = Random.Range(-5f, 3f);
        }
 


        RPC_TypeQuestion(id, x1,z1,x2,z2);
    }

    [Rpc]
    public void RPC_TypeQuestion(int index , float x1, float z1, float x2, float z2)
    {
        PlatformTrue.SetActive(false);
        PlatformFalse.SetActive(false);

        if(Round > 1)
        {
            PlatformTrue.transform.localScale -= new Vector3(0.2f, 0, 0.2f);
            PlatformFalse.transform.localScale -= new Vector3(0.2f, 0, 0.2f);
        }
        else
        {
            PlatformTrue.transform.localScale = new Vector3(3.5f, 1.3f, 3.5f);
            PlatformFalse.transform.localScale = new Vector3(3.5f, 1.3f, 3.5f);
        }

        if (Round == 2)
        {
            BGM_Fun.Stop();
            BGM_Dramatic.Play();
        }

        IPU.SetActive(true);
        Floor.gameObject.SetActive(true);

        PlatformTrue.transform.position = new Vector3(x1, PlatformTrue.transform.position.y, z1);
        PlatformFalse.transform.position = new Vector3(x2, PlatformFalse.transform.position.y, z2);

        StartCoroutine(CR_TypeQuestion(index));
    }

    public IEnumerator CR_TypeQuestion(int index)
    {
        yield return new WaitForSeconds(2f);

        string question = QuestionPool[index].question_string;
        string typeQuestion = "";
        int i = 0;
        while(typeQuestion != question)
        {
          
            yield return new WaitForSeconds(typingSpeed);
            typeQuestion += question[i];
            TMP_Question.text = typeQuestion;
            i++;
        }

        RPC_UpdateTimer(7);
        PlatformTrue.SetActive(true);
        PlatformFalse.SetActive(true);

        TMP_Time.gameObject.SetActive(true);
        if (Runner.IsSharedModeMasterClient)
        {
            yield return StartCoroutine(CR_StartTimer());
            RPC_Drop(QuestionPool[index].answer);

        }



    }
    public IEnumerator CR_StartTimer()
    {
        int time = 7;
        RPC_UpdateTimer(time);
        while (time != 0)
        {
            yield return new WaitForSeconds(1f);
            time--;
            RPC_UpdateTimer(time);
        }



        yield return null;
    }
    [Rpc]
    public void RPC_Drop(bool condition)
    {
        TMP_Time.gameObject.SetActive(false);
        Floor.SetActive(false);
        if (condition)
        {
            PlatformTrue.SetActive(true);
            PlatformFalse.SetActive(false);

        }
        else
        {
            PlatformTrue.SetActive(false);
            PlatformFalse.SetActive(true);
        }
        typingSpeed -= 0.02f;
        Round++;
        if (Runner.IsSharedModeMasterClient)
        {
            StartCoroutine(CR_WaitNextQuestion());
        }
    }

    public IEnumerator CR_WaitNextQuestion()
    {
        yield return new WaitForSeconds(3f);
        bool endGame = true;
        int aliveCounter = 0;
        foreach (PlayerRef player in D_playerObjects.Keys)
        {
            if (D_playerObjects[player].isDeath == false)
            {
                aliveCounter++;
            }
        }

        if (aliveCounter <= 1){
            RPC_Reset();
        }
        else
        {
            StartQuestion();
        }

    }

    [Rpc]
    public void RPC_UpdateTimer(int time)
    {
        TMP_Time.text = time.ToString();
    }

   

    #endregion
}
