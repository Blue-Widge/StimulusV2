using UnityEngine;
using System.Collections;

public class AirComprised : MonoBehaviour {
	comSocket m_comScript;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter()
	{
		m_comScript = GameObject.Find ("Wagon_Player").GetComponent<comSocket>();
		m_comScript.m_airComprised = true;
	}

}
