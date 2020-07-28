using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour {

    protected void Awake() {
        SetPointEnabled(false);
    }

    public CompoundObject GetCompound() {
        return transform.parent.transform.parent.GetComponent<CompoundObject>();
    }

    public void SetColor(Color c) {
        GetComponent<Renderer>().material.color = c;
    }

    public void SetPointEnabled(bool set) {
        transform.GetComponent<Renderer>().enabled = set;
        //transform.GetComponent<SphereCollider>().enabled = set;
    }
}
