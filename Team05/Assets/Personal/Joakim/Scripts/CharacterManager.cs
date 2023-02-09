using System;
using System.Collections.Generic;
using Andreas.Scripts;
using FlowFieldSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterManager : MonoBehaviour
{
    public bool rangedLockedIn, meleeLockedIn;
    public GameObject characterSelectionScreen;
    public GameObject mainGameUI;
    public List<Player> Players = new List<Player>();


    [SerializeField] private Transform _spawnPoint1;
    [SerializeField] private Transform _spawnPoint2;


    // public static CharacterManager Instance = null;

    private void Awake()
    {
        // if (Instance == null)
        // Instance = this;
        // else if (Instance != this)
        // Destroy(gameObject);
    }

    public void CheckIfAllAreLockedIn()
    {
        if(rangedLockedIn && meleeLockedIn)
        {
            characterSelectionScreen.SetActive(false);
            mainGameUI.SetActive(true);
            foreach(var player in Players)
            {
                player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
                player.AssignPlayerToRole(player.cType);
                player.HealthMaterial = Resources.Load<Material>("Player" + player._playerNumber + "Health");
                player.Health.ResetHealth(player);

                if(player._playerNumber == 1)
                {
                    player.otherPlayer = GameObject.FindWithTag("Player2");
                    player.transform.position = _spawnPoint1.position;
                }
                else if(player._playerNumber == 2)
                {
                    player.otherPlayer = GameObject.FindWithTag("Player1");
                    player.transform.position = _spawnPoint2.position;
                }

                GameManager.Instance.CameraController.SetTransforms(player.transform);
                var spawner = GameManager.Instance.EnemyManager.Spawner;
                spawner.EnableSpawning(true);
                spawner.AssignFlowField(player.GetComponentInChildren<FlowFieldManager>());
            }

            // GameManager.Instance.RopeManager.SetRopeEnds(Players[0].gameObject, Players[1].gameObject);
        }
    }
}