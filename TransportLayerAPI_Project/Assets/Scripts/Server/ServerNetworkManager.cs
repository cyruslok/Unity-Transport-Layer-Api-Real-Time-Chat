using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ServerNetworkManager : MonoBehaviour {

	public ServerUIManager UI_Manager;
	List<int> Clients_Id = new List<int>();
	int mServerSocket = -1;
	int mMaxConnections = 10;
	byte mChannelUnreliable;
	byte mChannelReliable;
	bool mHostClientInitialized = false;


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
		mServerSocket = NetworkTransport.AddHost ( ht , 8888  );

		// Check to make sure our socket formations were successful
		if( mServerSocket < 0 ){ UI_Manager.AddTextItem ("Server socket creation failed!"); } else { UI_Manager.AddTextItem ("Server socket creation successful!"); }

		mHostClientInitialized = true;

	}

	/// <summary>
	/// Log any network errors to the console.
	/// </summary>
	/// <param name="error">Error.</param>
	void LogNetworkError(byte error){
		if( error != (byte)NetworkError.Ok){
			NetworkError nerror = (NetworkError)error;
			UI_Manager.AddTextItem ("Error: " + nerror.ToString ());
			UI_Manager.AddTextItem("Error: " + nerror.ToString ());
		}
	}


	// Update is called once per frame
	void Update () {

		if(!mHostClientInitialized){
			return;
		}

		int SocketId; 
		int connectionId;
		int channelId;
		int dataSize;
		byte[] buffer = new byte[1024];
		byte error;

		NetworkEventType networkEvent = NetworkEventType.DataEvent;

		// Poll both server/client events
		do
		{
			networkEvent = NetworkTransport.Receive( out SocketId , out connectionId , out channelId , buffer , 1024 , out dataSize , out error );

			switch(networkEvent){
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
				// Server received disconnect event
				if( SocketId == mServerSocket ){
					UI_Manager.AddTextItem ("Server: Player " + connectionId.ToString () + " connected!" );
					Clients_Id.Add(connectionId);
				}
				break;

			case NetworkEventType.DataEvent:
				// Server received data
				if( SocketId == mServerSocket ){
					// decode data
					Stream stream = new MemoryStream(buffer);
					BinaryFormatter f = new BinaryFormatter();
					string msg = f.Deserialize( stream ).ToString ();
					UI_Manager.AddTextItem ( connectionId.ToString () + " : " + msg );
					BroadcastData(connectionId.ToString () + " : " + msg);
				}
				break;

			case NetworkEventType.DisconnectEvent:
				// Server received disconnect event
				if( SocketId == mServerSocket ){
					UI_Manager.AddTextItem ("Server: Received disconnect from " + connectionId.ToString () );
					Clients_Id.Remove(connectionId);
				}
				break;
			}

		} while ( networkEvent != NetworkEventType.Nothing );
	}

	public void SendData(object data, int client_socket, int client_connection ){
		// Send the server a message
		byte error;
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter f = new BinaryFormatter();
		f.Serialize ( stream , data );
		NetworkTransport.Send ( client_socket , client_connection , mChannelReliable , buffer , (int)stream.Position , out error );
		LogNetworkError ( error );
	}

	public void BroadcastData(object data){
		foreach (int id in Clients_Id) {
			// Send the server a message
			byte error;
			byte[] buffer = new byte[1024];
			Stream stream = new MemoryStream(buffer);
			BinaryFormatter f = new BinaryFormatter();
			f.Serialize ( stream , data );
			NetworkTransport.Send ( mServerSocket , id , mChannelReliable , buffer , (int)stream.Position , out error );
			LogNetworkError ( error );
		}
	}

}