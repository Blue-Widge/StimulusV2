using UnityEngine;
using System.Collections;

public class flagChange : MonoBehaviour {

	public int m_flagId;
	public bool m_value;
	GameObject m_gameObject;
	Sounds m_soundScript;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter()
	{
		m_soundScript = GameObject.Find ("Wagon_Player").GetComponent<Sounds>();
		m_soundScript.m_flags[ m_flagId ] = m_value;
	}
}
