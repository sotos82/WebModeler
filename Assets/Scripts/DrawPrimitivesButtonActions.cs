using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawPrimitivesButtonActions : MonoBehaviour {

    protected Button DrawQuadButton;
    protected Button DrawCubeButton;

    protected void Awake() {
        DrawQuadButton = transform.Find(nameof(DrawQuadButton)).GetComponent<Button>();
        DrawCubeButton = transform.Find(nameof(DrawCubeButton)).GetComponent<Button>();

        DrawQuadButton.onClick.AddListener(DrawQuadButtonAction);
        DrawCubeButton.onClick.AddListener(DrawCubeButtonAction);
    }

    public void DrawQuadButtonAction() {
        ModelManager.instance.SetModelerState(ModelManager.ModelerState.DrawQuad);
    }

    public void DrawCubeButtonAction() {
        ModelManager.instance.SetModelerState(ModelManager.ModelerState.DrawCube);
    }

}
