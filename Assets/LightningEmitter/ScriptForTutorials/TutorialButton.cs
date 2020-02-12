using UnityEngine;
using System.Collections;

public class TutorialButton : MonoBehaviour {

    public GameObject particlePrefab;
    public Transform firstNode;
    public Transform lastNode;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnFire()
    {
        GameObject firedLtn = GameObject.Instantiate(particlePrefab);
        firedLtn.GetComponent<SoxLtn>().firstNode = firstNode;
        firedLtn.GetComponent<SoxLtn>().lastNode = lastNode;
    }
}
