using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientNetworkManager : MonoBehaviour {

	int mClientSocket = -1;
	int mClientConnection = -1;
	int mMaxConnections = 10;
	byte mChannelUnreliable;
	byte mChannelReliable;
	bool mClientConnected = false;

	// Use this for initialization
	void Start () {

		// Build global config
		GlobalConfig gc = new GlobalConfig();
		gc.ReactorModel = ReactorModel.FixRateReactor;
		gc.ThreadAwakeTimeout = 10;

		// Build channel config
		ConnectionConfig cc = new ConnectionConfig();
		mChannelReliable = cc.AddChannel (QosType.ReliableSequenced);
		mChannelUnreliable = cc.AddChannel(QosType.UnreliableSequenced);

		// Create host topology from config
		HostTopology ht = new HostTopology( cc , mMaxConnections  );

		// We have all of our other stuff figured out, so init
		NetworkTransport.Init(gc);

		// Open sockets for server and client
		mClientSocket = NetworkTransport.AddHost ( ht  );

		// Check to make sure our socket formations were successful
		if( mClientSocket < 0 ){ Debug.Log ("Client socket creation failed!"); } else { Debug.Log ("Client socket creation successful!"); }

		// Connect to server
		byte error;
		mClientConnection = NetworkTransport.Connect( mClientSocket , "127.0.0.1" , 8888 , 0 , out error );
		LogNetworkError( error );
	}

	void Update () {
		int recHostId; 
		int connectionId;
		int channelId;
		int dataSize;
		byte[] buffer = new byte[1024];
		byte error;

		NetworkEventType networkEvent = NetworkEventType.DataEvent;

		// Poll both server/client events
		do
		{
			networkEvent = NetworkTransport.Receive( out recHostId , out connectionId , out channelId , buffer , 1024 , out dataSize , out error );
			switch(networkEvent){
				case NetworkEventType.Nothing:
					break;
				case NetworkEventType.ConnectEvent:
					if( recHostId == mClientSocket ){
						Debug.Log ("Connected to Server " + connectionId.ToString () );
						mClientConnected = true; 
					}
					break;

				case NetworkEventType.DataEvent:
					if( recHostId == mClientSocket ){
						Stream stream = new MemoryStream(buffer);
						BinaryFormatter f = new BinaryFormatter();
						string msg = f.Deserialize( stream ).ToString ();
						Debug.Log ( msg );
					}
					break;

				case NetworkEventType.DisconnectEvent:
					// Client received disconnect event
					if( recHostId == mClientSocket && connectionId == mClientConnection ){
						Debug.Log ("Disconnected from server!");
						mClientConnected = false;
					}
					break;
			}
		} while ( networkEvent != NetworkEventType.Nothing );
	}

	void LogNetworkError(byte error){
		if( error != (byte)NetworkError.Ok){
			NetworkError nerror = (NetworkError)error;
			Debug.Log ("Error: " + nerror.ToString ());
		}
	}

	public void SendData(object data){
		// Send the server a message
		byte error;
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();
		f.Serialize ( stream , data );
		NetworkTransport.Send ( mClientSocket , mClientConnection , mChannelReliable , buffer , (int)stream.Position , out error );
		LogNetworkError ( error );
	}
}
