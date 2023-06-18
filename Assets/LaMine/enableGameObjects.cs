using UnityEngine;
using System.Collections;

public class enableGameObjects : MonoBehaviour {
	public GameObject[] m_gameObjects;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter()
	{
		for (int i = 0; i < m_gameObjects.Length; i++) 
		{
			//m_gameObjects [i] = true;
			m_gameObjects [i].SetActive (true);
		}

	}
}
