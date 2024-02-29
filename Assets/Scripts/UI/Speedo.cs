using Assets.Scripts.Movement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Speedo : MonoBehaviour
{
    PlayerController playerController;
    TextMeshProUGUI text;
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = playerController.Velocity.magnitude.ToString();
    }
}
