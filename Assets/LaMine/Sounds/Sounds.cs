using UnityEngine;
using System.Collections;

public class Sounds : MonoBehaviour {

	//background
	public AudioClip[] m_sounds;
	public AudioClip[] m_tempos;
	public bool[] m_flags;
	float m_expectedDuration;
	int m_soundId;
	bool m_backGroundFinished;

	//rails
	public bool m_railsSoundsEnabled;
	public AudioClip[] m_railsSounds;
	public int[] m_railsProbas;
	System.Random m_random;
	int m_maxProba;
	float m_lastTimeSound;
	float m_delay;
	float m_speed;
	follower m_follower;

	//subwoofer
	public AudioClip m_railsSubWoofer;

	//general
	public string m_gameObjectName;
	GameObject m_object;
	AudioSource[] m_sources;
	int m_backgroundId;
	int m_railsSoundsId;
	int m_subwooferId;
	public float m_railsVolume;
	public float m_backgroundVolume;

	// Use this for initialization
	void Start () {
		
		m_soundId = 0;
		m_backgroundId = 0;
		m_railsSoundsId = 1;
		m_subwooferId = 2;
		m_expectedDuration = 0.0f;
		m_random = new System.Random ();
		m_backGroundFinished = false;

		//add sounds coponents
		m_object = GameObject.Find (m_gameObjectName);
		// add the 3 audio source
		m_object.AddComponent<AudioSource> ();
		m_object.AddComponent<AudioSource> ();
		m_object.AddComponent<AudioSource> ();
		// add the audio listener
		m_object.AddComponent<AudioListener> ();
		m_sources = m_object.GetComponents<AudioSource> ();
		m_follower = m_object.GetComponent<follower> ();

		//manage background sounds
		m_sources [m_backgroundId].pitch = 0.89f;
		m_sources[m_backgroundId].enabled = true;
		m_sources [m_backgroundId].volume = m_backgroundVolume;
		m_sources[m_backgroundId].priority = 0;

		//manage rails sounds
		m_sources[m_railsSoundsId].enabled = true;
		m_sources[m_railsSoundsId].volume = m_railsVolume;
		m_sources[m_railsSoundsId].priority = 0;
		m_railsSoundsEnabled = true;
		if (m_railsSounds.Length != m_railsProbas.Length) {
			m_railsProbas = new int[ m_railsSounds.Length ];//get the same size
			for (int i = 0; i < m_railsProbas.Length; i++)
				m_railsProbas [i] = 1;//set the same proba for all sound
		} 
		m_maxProba = 0;
		for (int i = 0; i < m_railsProbas.Length; i++)
			m_maxProba += m_railsProbas [i];
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_follower == null || m_follower.m_isRunning == false)
			return;
		updateBackgroundSounds ();
		updateRailsSounds ();
	}

	void updateBackgroundSounds()
	{
		if (m_backGroundFinished == true)
			return ;

		//start the sound
		if (m_expectedDuration == 0.0f) 
		{
			m_sources[m_backgroundId].clip = (AudioClip)m_sounds [0];
			m_sources[m_backgroundId].Play ();
			m_expectedDuration = m_sounds [0].length;
		}

		//the sound will finish with this frame
		if ((GetComponent<AudioSource>().time + Time.deltaTime) > m_expectedDuration) 
		{
			float l_duration = 0.0f;
			float l_delay = m_expectedDuration - GetComponent<AudioSource>().time;

			//the point is not reached
			if (m_flags [m_soundId] == false) {
					m_sources [m_backgroundId].clip = (AudioClip)m_tempos [m_soundId];
					l_duration = m_tempos [m_soundId].length;
			} else {
				m_soundId++;
				if( m_soundId >= m_sounds.Length )
				{
					m_backGroundFinished = true;
					return ;
				}
				m_sources [m_backgroundId].clip = (AudioClip)m_sounds [m_soundId];
				l_duration = m_sounds [m_soundId].length;
			}

			//play the next sound just after this one
			m_sources [m_backgroundId].PlayDelayed (l_delay);

			//add the duration of the tempo or sound
			m_expectedDuration = l_duration;
		}
	}

	void updateRailsSounds()
	{
		int l_randValue = (int)(m_random.NextDouble () * (double)m_maxProba);
		int id = getSoundId (l_randValue);

		if (m_railsSoundsEnabled == false)
			return;
		
		m_speed = m_follower.vitesse;
		
		m_delay = 0.4f - (m_speed - 5.0f) * 0.016f;//1.0f - (m_speed / 25.0f);
		if (m_delay > 0.5f) m_delay = 0.5f;
		if (m_delay < 0.18f) m_delay = 0.18f;

		if ((Time.time - m_lastTimeSound) > m_delay && m_sources[m_railsSoundsId].isPlaying == false ) 
		{
			m_sources[m_railsSoundsId].PlayOneShot( m_railsSounds[ id ] ); 
			m_sources[m_subwooferId].PlayOneShot( m_railsSubWoofer ); 
			m_lastTimeSound = Time.time;
		}
	}

	int getSoundId( int p_proba )
	{
		int i;
		for ( i = 0 ; i < m_railsProbas.Length - 1 ; i++) 
		{
			if( p_proba < m_railsProbas[i] )
				return i;
		}
		return m_railsProbas.Length - 1;
	}
}
