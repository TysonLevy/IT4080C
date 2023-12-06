using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour {
    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    public NetworkVariable<Color> playerColorNetVar;
    public BulletSpawner bulletSpawner;
    public NetworkVariable<int> ScoreNetVar = new NetworkVariable<int>();

    private Camera playerCamera;
    private GameObject playerbody;

    public NetworkVariable<int> playerHP = new NetworkVariable<int>();

    private void NetworkInit() {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = !IsOwner;

        playerbody = transform.Find("PlayerBody").gameObject;
        ApplyColor();
        playerColorNetVar.OnValueChanged += OnPlayerColorChanged;

        if (IsClient) {
            ScoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
    }

    private void Awake() {
        NetworkHelper.Log(this, "Awake");
    }

    private void Start() {
        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn() {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
        playerHP.Value = 100;
    }

    private void OnCollisionEnter(Collision collision) { 
        if(IsServer) {
            ServerHandleCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsServer) return;
        
        if(other.CompareTag("power_up")) {
            other.GetComponent<BasePowerUp>().ServerPickUp(this);
        }

        if(other.GetComponent<HealthPickup>()) {
            if (playerHP.Value <= 50) playerHP.Value += 50;
            else playerHP.Value = 100;
        }
    }

    private void ServerHandleCollision(Collision collision) {
        if (collision.gameObject.CompareTag("bullet")) {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            NetworkHelper.Log(this, $"Hit by {collision.gameObject.name} " + $"owned by {ownerId}");
            Player other = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            other.ScoreNetVar.Value += 1;
            playerHP.Value -= 10;
            NetworkHelper.Log(this, "was hit");
            Destroy(collision.gameObject);
        }
    }

    private void Update() {
        if (IsOwner) {
            OwnerHandleInput();
            if (Input.GetButtonDown("Fire1")) { 
                bulletSpawner.FireServerRpc();
            }
        }
    }

    private void OwnerHandleInput() {
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();
        if(movement != Vector3.zero || rotation != Vector3.zero) {
            MoveServerRpc(movement, rotation, NetworkManager.LocalClientId);
        }
        
    }

    private void ApplyColor() {
        playerbody.GetComponent<MeshRenderer>().material.color = playerColorNetVar.Value;
    }

    public void OnPlayerColorChanged(Color previous, Color changed) { 
        ApplyColor();
    }

    private void ClientOnScoreValueChanged(int old, int current) { 
        if (IsOwner) {
            NetworkHelper.Log(this, $"My score is {ScoreNetVar.Value}");
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation, ulong clientId)
    {
        transform.Translate(movement);
        transform.Rotate(rotation);
        if (NetworkManager.LocalClientId != clientId)
        {
            if (playerbody.transform.position.x < -25)
            {
                transform.position = new Vector3(-25, transform.position.y, transform.position.z);
            }
            if (playerbody.transform.position.x > 25)
            {
                transform.position = new Vector3(25, transform.position.y, transform.position.z);
            }
            if (playerbody.transform.position.z < -25)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -25);
            }
            if (playerbody.transform.position.z > 25)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 25);
            }
        }
    }

    // Rotate around the y axis when shift is not pressed
    private Vector3 CalcRotation()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown)
        {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }


    // Move up and back, and strafe when shift is pressed
    private Vector3 CalcMovement()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }

}


