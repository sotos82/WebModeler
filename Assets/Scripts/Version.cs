﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour {

    protected void Awake() {
        GetComponent<Text>().text = "Version " + Application.version;
    }

}
