using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ReadyTriggerArea : MonoBehaviour
{
    // Start is called before the first frame update

    Coroutine cr_waitingForDataManager;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<PlayerBehaviour>() != null)
        {
            PlayerBehaviour pb = other.gameObject.GetComponent<PlayerBehaviour>();
            if (pb.Runner.LocalPlayer == pb.Object.StateAuthority)
            {
                if (cr_waitingForDataManager != null)
                {
                    StopCoroutine(cr_waitingForDataManager);
                }
                cr_waitingForDataManager = StartCoroutine(CR_FindDataManager(pb.Runner.LocalPlayer));
            }
        }
        
    }

    public IEnumerator CR_FindDataManager(PlayerRef player)
    {
        while(FindObjectOfType<DataManager>() == null)
        {
            yield return null;
        }
        FindObjectOfType<DataManager>().RPC_SetReady(player);

        yield return null;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerBehaviour>() != null)
        {
            PlayerBehaviour pb = other.gameObject.GetComponent<PlayerBehaviour>();
            if (pb.Runner.LocalPlayer == pb.Object.StateAuthority)
            {
                FindObjectOfType<DataManager>().RPC_SetNotReady(pb.Runner.LocalPlayer);
            }
        }
    }

}
