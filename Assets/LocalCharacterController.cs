using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCharacterController : MonoBehaviour
{
    // Start is called before the first frame update
    CharacterController cc;
    public float Speed;
    public GameObject MainCamera;
    private void Awake()
    {
        // get a reference to our main camera

    }
    void Start()
    {
        MainCamera = Camera.main.gameObject;
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            inputDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDirection += Vector3.right;
        }
        MainCamera.transform.LookAt(transform, Vector3.forward) ;
        cc.Move(inputDirection * Speed * Time.deltaTime);

    }
}
