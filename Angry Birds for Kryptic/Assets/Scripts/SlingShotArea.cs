using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotArea : MonoBehaviour
{
    public LayerMask slingshotAreaMask;
    public bool IsWithinSlingshotArea(){
        Vector2 worldPosition =  Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if (Physics2D.OverlapPoint(worldPosition, slingshotAreaMask)){
            return true;
        }
        else{
            return false;
        }
    }
}
