using System;
using UnityEngine;

public class DebugCommandsExample : MonoBehaviour {
    private void OnEnable() {
        DebugCommands.Instance.AddDebugCommand(SpawnCube, KeyCode.A);
    }

    private void OnDisable() {
        
    }

    private void SpawnCube() {
        GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
}