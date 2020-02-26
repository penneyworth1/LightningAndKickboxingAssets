using UnityEngine;
using System.Collections;

public class SpiralRotate : MonoBehaviour {

    public Vector3 rotSpd = new Vector3(0, 0, 200.0f);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(Time.smoothDeltaTime * rotSpd.x, Time.smoothDeltaTime * rotSpd.y, Time.smoothDeltaTime * rotSpd.z);
	}
}
