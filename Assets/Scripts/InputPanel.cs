using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour {

    public InputField InputFieldMagnitude { protected set; get; }

    public void SetMagnitudeFieldActive(bool set) {
        InputFieldMagnitude.gameObject.SetActive(set);
    }

    public void SetMagnitudeFieldText(string s) {
        InputFieldMagnitude.text = s;
    }

    protected void Awake() {

        InputFieldMagnitude = transform.Find(nameof(InputFieldMagnitude)).GetComponent<InputField>();

        InputFieldMagnitude.text = "test";

        InputFieldMagnitude.onValueChanged.AddListener(InputFieldMagnitudeChanged);

        SetMagnitudeFieldActive(false);

    }

    public void InputFieldMagnitudeChanged(string s) {
        //print(s);
    }

}
