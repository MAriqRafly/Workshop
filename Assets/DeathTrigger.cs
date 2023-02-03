using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource Waa;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if (other.gameObject.GetComponent<PlayerBehaviour>() != null)
        {
            Waa.Play();
            PlayerBehaviour pb = other.gameObject.GetComponent<PlayerBehaviour>();
            pb.AllowMovement = false;
            if (pb.Runner.LocalPlayer == pb.Object.StateAuthority)
            {
                FindObjectOfType<DataManager>().RPC_SetDeath(pb.Runner.LocalPlayer);
            }
        }
    }

 
}
