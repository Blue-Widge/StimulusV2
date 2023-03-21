using UnityEngine;
using System.Collections;

public class lampBlinking : MonoBehaviour {
	comSocket m_comScript;
	private bool m_isRunning;
	public int m_nbBlink = 0;
	public float m_duration = 1.0f;
	float m_delay;
	float m_time, m_lastTime = 0.0f;
	Light m_light;
	bool m_isOneShot;
	// Use this for initialization
	void Start () {
		m_isRunning = false;
		m_isOneShot = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_isRunning == true) 
		{
			m_time = Time.time;

			if( (m_time - m_lastTime) > m_delay )
			{
				m_comScript.m_lampState = !m_comScript.m_lampState;
				m_light.enabled = m_comScript.m_lampState;
				m_nbBlink--;
				m_lastTime = m_time;
			}

			if( m_nbBlink == 0 )
			{
				if( m_isOneShot == false )
				{
					m_light.enabled = true;
					m_comScript.m_lampState = true;
				}
				m_isRunning = false;
			}
		}
	}

	public void OnTriggerEnter()
	{
		m_comScript = GameObject.Find ("Wagon_Player").GetComponent<comSocket>();
		m_light = GameObject.Find ("Spotlight").GetComponent<Light> ();
		m_isRunning = true;
		m_delay = m_duration / m_nbBlink;
		m_isOneShot = false;
		if (m_nbBlink == 1)
			m_isOneShot = true;
	}
}
