using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class comSocket : MonoBehaviour {
	
	follower m_follower;
	GameObject m_gameObject;
	public string m_gameObjectName;
	TcpClient m_client;
	StreamWriter m_writer;
	NetworkStream m_stream;
	Vector3 m_rotation;
	byte[] m_buffer;
	byte m_checksum;
	public byte m_leftBrakes = 0;
	public byte m_rightBrakes = 0;
	public bool m_brakesDestroyed = false;
	bool m_lastBrakesDestroyed = false;
	public bool m_shock = false;
	public bool m_airComprised = false;
	bool m_isConnectOk;
	System.Threading.Thread m_thread = null;
	int m_brakesValue;
	public float m_maxZUnityRotation = 0.0f;
	public float m_maxXUnityRotation = 0.0f;
	public float m_maxZRealRotation = 0.0f;
	public float m_maxXRealRotation = 0.0f;
	public float m_maxBrakesAngle = 0.0f;
	float m_xCoef;
	float m_zCoef;
	//CSV
	//StreamWriter m_file;

	// Use this for initialization
	void Start () 
	{
		m_client = new TcpClient();
		m_client.Connect ("127.0.0.1", 80);
		if ( !m_client.Connected )
			Debug.Log ("Connection failed");
		m_stream = m_client.GetStream();
		m_buffer = new byte[ 14 ];
		m_isConnectOk = true;
		m_thread = new System.Threading.Thread (routineReception);
		m_thread.Start ();
		m_gameObject = GameObject.Find (m_gameObjectName);
		m_follower = m_gameObject.GetComponent<follower> ();

		m_xCoef = m_maxXRealRotation / m_maxXUnityRotation;
		m_zCoef = m_maxZRealRotation / m_maxZUnityRotation;
		m_brakesValue = 0;

		//CSV
		//m_file = null;
		//m_file = new StreamWriter ("angles.csv");
		//m_file.Write ("posX" + ";" + "Acc" + "\n");
		//m_file.Write ("Real x" + ";" + "Real z" + "\n" );
	}
	
	// Update is called once per frame
	void Update () 
	{
		int i;
		short l_tmpShort;
		float l_speed = 0.0f;
		float l_x, l_z;

		if( m_follower != null )
			l_speed = m_follower.vitesse;

		//m_file.Write (m_follower.m_x.ToString () + ";" + m_follower.m_acceleration.ToString () + "\n");

		if (m_client.Connected == false || m_isConnectOk == false)
			return;
		
		m_rotation = transform.rotation.eulerAngles;
		
		//
		// Speed (fans control)
		//
		
		//frame type
		m_buffer [0] = 0x03;
		//size
		m_buffer [1] = 0x01;
		m_buffer [2] = 0x00;
		m_buffer [3] = 0x00;
		m_buffer [4] = 0x00;
		try {
			m_stream.Write (m_buffer, 0, 5);
		} catch (IOException e) {
			m_isConnectOk = false;
			return;
		}
		
		//apply a coef and get the speed positive
		if (l_speed > 0.0f) 
		{
			l_speed *= 2.0f;
		} 
		else 
		{
			l_speed *= -2.0f;
		}
		if( l_speed > 100.0f ) 
			l_speed = 100.0f;
		m_buffer[0] = (byte)l_speed;
		try{
			m_stream.Write( m_buffer, 0, 1 );
		}
		catch(IOException e)
		{
			m_isConnectOk = false;
			return ;
		}
		
		
		//
		// Rotation
		//
		
		//frame type
		m_buffer [0] = 0x04;
		//size
		m_buffer [1] = 0x05;
		m_buffer [2] = 0x00;
		m_buffer [3] = 0x00;
		m_buffer [4] = 0x00;
		try
		{
			m_stream.Write( m_buffer, 0, 5 );
		}
		catch(IOException e)
		{
			m_isConnectOk = false;
			return ;
		}

		l_x = m_rotation.x;
		l_z = -m_rotation.z;

		//put angles in [-PI;PI]
		if (l_x > 180.0f) l_x -= 360.0f;
		if (l_z > 180.0f) l_z -= 360.0f;

		//scale the angles
		l_x *= m_xCoef;
		l_z *= m_zCoef;

		//add the acceleration
		l_z += (m_follower.m_acceleration > 0.0f) ? m_follower.m_acceleration : 0.0f;

		//security
		if (l_x > m_maxXRealRotation) l_x = m_maxXRealRotation;
		if (l_x < -m_maxXRealRotation) l_x = -m_maxXRealRotation;
		if (l_z > m_maxZRealRotation) l_z = m_maxZRealRotation;
		if (l_z < -m_maxZRealRotation) l_z = -m_maxZRealRotation;

		//add the brakes
		l_x += (float)( m_brakesValue / 100.0f ) * m_maxBrakesAngle;

		//CSV
		//m_file.Write (l_x.ToString () + ";" + l_z.ToString () + "\n" );

		//convert the value in integer
		l_tmpShort = (short)(l_z * 10.0f);
		m_buffer [0] = (byte)(l_tmpShort & 0x00FF);
		m_buffer [1] = (byte)((l_tmpShort & 0xFF00) >> 8);
		
		l_tmpShort = (short)(l_x * 10.0f);
		m_buffer [2] = (byte)(l_tmpShort & 0x00FF);
		m_buffer [3] = (byte)((l_tmpShort & 0xFF00) >> 8);
		
		m_checksum = m_buffer [0];
		for (i = 1; i < 4; i++)
			m_checksum ^= m_buffer [i];
		m_buffer [4] = m_checksum;
		
		try{
			m_stream.Write( m_buffer, 0, 5 );
		}
		catch(IOException e)
		{
			m_isConnectOk = false;
			return ;
		}

		//
		// Shock
		//
		if (m_shock == true) 
		{
			m_shock = false;
			//frame type
			m_buffer [0] = 0x0B;
			//size
			m_buffer [1] = 0x01;
			m_buffer [2] = 0x00;
			m_buffer [3] = 0x00;
			m_buffer [4] = 0x00;
			m_buffer [5] = 0x01;
			try 
			{
				m_stream.Write (m_buffer, 0, 6);
			} 
			catch (IOException e) 
			{
				m_isConnectOk = false;
			}
		} 

		//
		// Brakes destroyed
		//
		if (m_brakesDestroyed != m_lastBrakesDestroyed) 
		{
			//frame type
			m_buffer [0] = 0x0A;
			//size
			m_buffer [1] = 0x01;
			m_buffer [2] = 0x00;
			m_buffer [3] = 0x00;
			m_buffer [4] = 0x00;
			m_buffer [5] = (m_brakesDestroyed == true) ? (byte)1 : (byte)0;
			try 
			{
				m_stream.Write (m_buffer, 0, 6);
			} 
			catch (IOException e) 
			{
				m_isConnectOk = false;
			}
		}

		//
		// Air comprised (bats)
		//
		if (m_airComprised == true) 
		{
			m_airComprised = false;
			//frame type
			m_buffer [0] = 0x0C;
			//size
			m_buffer [1] = 0x01;
			m_buffer [2] = 0x00;
			m_buffer [3] = 0x00;
			m_buffer [4] = 0x00;
			m_buffer [5] = 0x01;
			try 
			{
				m_stream.Write (m_buffer, 0, 6);
			} 
			catch (IOException e) 
			{
				m_isConnectOk = false;
			}
		}
		m_lastBrakesDestroyed = m_brakesDestroyed;
	}
	
	void routineReception()
	{
		int l_state = 0;
		byte[] l_tab = new byte[1];
		byte l_leftBrakes = 0, l_rightBrakes = 0;
		
		while( true )
		{
			if( m_stream.DataAvailable )
			{
				m_stream.Read( l_tab, 0, 1);
				switch( l_state )
				{
				case 0:
					if( l_tab[0] == 0x01 )//start of trame
						l_state = 1;
					else
						l_state = 0;
					break;
				case 1:
					if( l_tab[0] == 0x05 )//brakes trame id
						l_state = 2;
					else
						l_state = 0;
					break;
				case 2:
					l_leftBrakes = l_tab[0];
					l_state = 3;
					break;
				case 3:
					l_rightBrakes = l_tab[0];
					l_state = 4;
					break;
				case 4:
					if( l_tab[0] == 0x02 )//end of trame
					{
						m_leftBrakes = l_leftBrakes;
						m_rightBrakes = l_rightBrakes;
						m_follower.m_brakesPower = ((m_leftBrakes + m_rightBrakes) / 2);
						m_follower.m_brakesValue = (m_leftBrakes - m_rightBrakes);
						m_brakesValue = m_follower.m_brakesValue;
					}
					l_state = 0;
					break;
				default:
					break;
				}
			}
			System.Threading.Thread.Sleep( 1 );
		}
	}
	
	void OnApplicationQuit ()
	{
		byte[] l_tab = new byte[5];
		l_tab[0] = 0xFF;

		//CSV
		//m_file.Close ();

		if (m_thread != null) 
		{
			//ask for closing
			m_thread.Abort ();
			
			//wait to kill
			m_thread.Join ();
		}
		
		//Server gone 
		if (m_client == null || m_client.Connected == false || m_isConnectOk == false)
			return;
		
		if( m_stream != null )//inform the server that the client is gone
			m_stream.Write( l_tab, 0, l_tab.Length );
	}
}
