using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour {
    [SerializeField]
    private ScrollLoopController scroll;
	// Use this for initialization
	void Start () {
        List<int> list = new List<int>();
        for(int i=0; i< 100; i++) {
            list.Add(i);
        }
        scroll.initWithData(list);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
