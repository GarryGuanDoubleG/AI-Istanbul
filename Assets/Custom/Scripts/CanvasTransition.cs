using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasTransition : MonoBehaviour {

    public UIManager.ScreenState transitionState;
    UIManager.OnTransitionHandler transitionHandler;

    public void OnClick()
    {
        transitionHandler(transitionState);
    }
	// Use this for initialization
	void Start () {
        transitionHandler = UIManager.GetTransitionHandler();
    }
	
}
