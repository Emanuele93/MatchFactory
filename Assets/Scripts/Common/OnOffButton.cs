using UnityEngine.UI;
using UnityEngine;
using System;

namespace Common
{
	public class OnOffButton : MonoBehaviour
	{
		[SerializeField] private Image image;
		[SerializeField] private Sprite onSprite;
		[SerializeField] private Sprite offSprite;

		public Action<bool> OnStateChange;
		
		public void SetState(bool state)
		{
			image.sprite = state ? onSprite : offSprite;
		}
		
		public void ChangeState()
		{
			var newState = image.sprite == offSprite;
			image.sprite = newState ? onSprite : offSprite;
			OnStateChange.Invoke(newState);
		}
	}
}
