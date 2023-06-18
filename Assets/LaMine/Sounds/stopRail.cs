using UnityEngine;
using System.Collections;

public class stopRail : MonoBehaviour {

	GameObject m_gameObject;
	Sounds m_soundScript;
	comSocket m_comSocket;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void OnTriggerEnter()
	{
		m_soundScript = GameObject.Find ("Wagon_Player").GetComponent<Sounds>();
		m_soundScript.m_railsSoundsEnabled = false;
		m_comSocket = GameObject.Find ("Wagon_Player").GetComponent<comSocket>();
		m_comSocket.m_stop = true;
	}
}
