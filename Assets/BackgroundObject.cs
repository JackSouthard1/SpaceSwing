using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObject : MonoBehaviour {
	Color backgroundColor = new Color (29f/255f, 34f/255f, 64f/255f, 1f);

	SpriteRenderer myRenderer;
	Shader shaderGUItext;
	Shader shaderSpritesDefault;

	void Start () {
		myRenderer = gameObject.GetComponent<SpriteRenderer>();
		shaderGUItext = Shader.Find("GUI/Text Shader");
		shaderSpritesDefault = Shader.Find("Sprites/Default"); // or whatever sprite shader is being used

		myRenderer.material.shader = shaderGUItext;
		myRenderer.color = backgroundColor;
	}
}
