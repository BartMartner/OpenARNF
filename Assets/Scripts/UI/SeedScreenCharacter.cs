using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedScreenCharacter : MonoBehaviour
{
    public char character;
    public Button button;
    public Text text;    

    public void Awake()
    {
        text = GetComponent<Text>();
        button = GetComponent<Button>();
    }
}
