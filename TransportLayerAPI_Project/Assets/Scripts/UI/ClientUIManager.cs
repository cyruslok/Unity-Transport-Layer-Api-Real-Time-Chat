using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientUIManager : MonoBehaviour {

	public ClientNetworkManager Client_Network_Manager;
	public GameObject Content_Rect;
	public GameObject Text_Item;
	public Button Send_Button;
	public Button Gen_Button;
	public InputField Send_Field;
	public ScrollRect ScrollView;

	private bool isGen = false ;

	void Start(){
		Send_Button.onClick.AddListener (() => SendOnClick ());
		Gen_Button.onClick.AddListener (() => GenOnClick ());
	}

	void Update(){
		if (isGen) {
			Client_Network_Manager.SendData ( new Vector3(Random.Range(0, 10000f),Random.Range(0, 10000f),Random.Range(0, 10000f)).ToString() );
		}
	}

	void SendOnClick(){
		Client_Network_Manager.SendData (Send_Field.text);
	}

	void GenOnClick(){
		isGen = !isGen;
	}

	public void AddTextItem(string msg){
		Text ui_text = Instantiate (Text_Item, Content_Rect.transform.position, Quaternion.identity, Content_Rect.transform).GetComponent<Text>();
		ui_text.text = msg;
		ScrollView.normalizedPosition = new Vector2(0, 0);
		if (Content_Rect.transform.childCount > 2000) {
			Destroy(Content_Rect.transform.GetChild(1).gameObject);
		}
	}
}
