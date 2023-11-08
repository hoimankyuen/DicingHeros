using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageDatabase : MonoBehaviour
{
	[System.Serializable]
	public class SpriteSet
	{
		public string name;
		public Sprite sprite;
	}
	public Image Image { get; protected set; }
	public List<SpriteSet> sprites = new List<SpriteSet>();
	protected Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

	// ========================================================= Monobehaviour Methods =========================================================

	/// <summary>
	/// Awake is called when the game object was created. It is always called before start and is 
	/// independent of if the game object is active or not.
	/// </summary>
	void Awake()
    {
		foreach (SpriteSet spriteSet in sprites)
		{
			spriteDictionary[spriteSet.name] = spriteSet.sprite;
		}
		Image = GetComponent<Image>();
	}

	// ========================================================= Functionalities =========================================================

	/// <summary>
	/// Set the sprite of the accociated image by name in the database.
	/// </summary>
	public void SetSpriteByName(string name)
	{
		if (spriteDictionary.ContainsKey(name))
		{
			Image.sprite = spriteDictionary[name];
		}
		else
		{
			Image.sprite = null;
		}
	}
}
