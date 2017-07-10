using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerUIManager : MonoBehaviour {

	public ScrollRect ScrollView;
	public GameObject Content_Rect;
	public GameObject Text_Item;

	public void AddTextItem(string msg){
		Text ui_text = Instantiate (Text_Item, Content_Rect.transform.position, Quaternion.identity, Content_Rect.transform).GetComponent<Text>();
		ui_text.text = msg;
		ScrollView.normalizedPosition = new Vector2(0, 0);
		if (Content_Rect.transform.childCount > 2000) {
			Destroy(Content_Rect.transform.GetChild(1).gameObject);
		}
	}
}
