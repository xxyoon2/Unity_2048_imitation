using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public bool up { get; private set; }
    public bool down { get; private set; }
    public bool left { get; private set; }
    public bool right { get; private set; }

    void Update()
    {
        up = down = left = right = false;

        up = Input.GetKey(KeyCode.UpArrow);
        down = Input.GetKey(KeyCode.DownArrow);
        left = Input.GetKey(KeyCode.LeftApple);
        right = Input.GetKey(KeyCode.RightArrow);
    }
}
