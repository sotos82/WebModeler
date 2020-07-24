using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour {

    public InputField InputFieldMagnitude { protected set; get; }
    public Button OkButton { protected set; get; }

    protected void Awake() {

        InputFieldMagnitude = transform.Find(nameof(InputFieldMagnitude)).GetComponent<InputField>();

        InputFieldMagnitude.text = "test";

        InputFieldMagnitude.onValueChanged.AddListener(InputFieldMagnitudeChanged);

        OkButton = transform.Find(nameof(OkButton)).GetComponent<Button>();
        OkButton.onClick.AddListener(OkButtonAction);

        SetAllActive(false);
    }

    public void SetAllActive(bool set) {
        if (InputFieldMagnitude.gameObject.activeSelf != set) {
            InputFieldMagnitude.gameObject.SetActive(set);
            OkButton.gameObject.SetActive(set);
            InputFieldMagnitude.text = "";
        }
    }

    public void SetMagnitudeFieldActive(bool set) {
        OkButton.gameObject.SetActive(false);
        if (InputFieldMagnitude.gameObject.activeSelf != set) {
            InputFieldMagnitude.gameObject.SetActive(set);
            InputFieldMagnitude.text = "";
        }
    }

    public void SetMagnitudeFieldText(string s) {
        InputFieldMagnitude.text = s;
    }

    public void InputFieldMagnitudeChanged(string s) {

    }

    public void OkButtonAction() {
        if (float.TryParse(InputFieldMagnitude.text, out float magnitude)) {
            if (magnitude != 0) {
                ModelManager.instance.ExtrudeFaceOnce(magnitude, false);
            }
        }
    }
}
