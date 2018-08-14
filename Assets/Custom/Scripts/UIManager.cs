using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Examples;
using Mapbox.Unity.Map;

public class UIManager : MonoBehaviour {

    public float maxIdleTime;
    public float idleToNewSessionTime;
    private float idleTimeCounter;

    public Camera camera;
    public CameraMovement camMovement;

    public AbstractMap map;

    [System.Serializable]
    public enum ScreenState
    {
        ScreenSaver,
        Explanation,
        Tutorial,
        Question,
        MapUI,
        None,
    };


    [System.Serializable]
    public struct CanvasEntry{
        public ScreenState name;
        public Canvas canvas;
    }

    public CanvasEntry[] canvasArray;
    private Dictionary<ScreenState, Canvas> canvasDict;
    private Canvas activeCanvas;
    private ScreenState currState;
    private ScreenState nextState;

    public delegate void OnTransitionHandler(UIManager.ScreenState newState);
    static OnTransitionHandler transitionHandler;

    public delegate void OnSelectDistrictHandler(string district, Mapbox.Utils.Vector2d LongLat);
    static OnSelectDistrictHandler selectDistrictHandler;

    public static OnTransitionHandler GetTransitionHandler()
    {
        return transitionHandler;
    }

    public static OnSelectDistrictHandler GetSelectDistrictHandler()
    {
        return selectDistrictHandler;
    }

    private void Awake()
    {
        if (transitionHandler == null)
            transitionHandler = UpdateActiveState;

        if (selectDistrictHandler == null)
            selectDistrictHandler = OnSelectDistrict;
    }

    void Start () {

		if(camera == null)
            Debug.LogError("Camera cannot be null");

        canvasDict = new Dictionary<ScreenState, Canvas>(canvasArray.Length);
        foreach (CanvasEntry entry in canvasArray)
        {
            canvasDict.Add(entry.name, entry.canvas);
            entry.canvas.gameObject.SetActive(false);            
        }

        nextState = ScreenState.None;
        if (canvasDict.ContainsKey(ScreenState.ScreenSaver))
        {
            activeCanvas = canvasDict[ScreenState.ScreenSaver];
            activeCanvas.gameObject.SetActive(true);
            currState = ScreenState.ScreenSaver;            
        }
	}
	
    void SetActiveCanvas(ScreenState canvasType)
    {
        activeCanvas.gameObject.SetActive(false);
        activeCanvas = canvasDict[canvasType];
        activeCanvas.gameObject.SetActive(true);
    }

    public void OnSelectDistrict(string district, Mapbox.Utils.Vector2d LongLat)
    {
        camMovement.MoveTo(map.GeoToWorldPosition(LongLat));
        map.UpdateMap(LongLat, map.AbsoluteZoom);
        UpdateActiveState(ScreenState.MapUI);
    }
   
    public void UpdateActiveState(ScreenState newState)
    {
        if (currState == newState) return;

        switch (currState)
        {
            case ScreenState.Question:
                //track question answers until deletion
                camMovement.SetCanMove(true);
                break;
            default:
                break;
        }

        switch(newState)
        {
            case ScreenState.Question:
                //track question answers until deletion
                camMovement.SetCanMove(false);
                break;
            default:
                break;
        }

        SetActiveCanvas(newState);
        currState = newState;
    }

	// Update is called once per frame
	void Update () {
        idleTimeCounter += Time.deltaTime;

        //TODO put in own function and add comments
        if (Input.anyKey || (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0) || Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            idleTimeCounter = 0.0f;
            if (currState == ScreenState.ScreenSaver)
            {
                if (nextState == ScreenState.None)
                    UpdateActiveState(ScreenState.Explanation);
                else
                    UpdateActiveState(nextState);
            }
        }
        else if (idleTimeCounter >= maxIdleTime)
        {
            if (currState != ScreenState.ScreenSaver)
            {
                nextState = currState;//go back to where we were
                UpdateActiveState(ScreenState.ScreenSaver);
            }
            
            if(idleTimeCounter >= idleToNewSessionTime)
            {
                nextState = ScreenState.None;
            }
                
        }
	}
}
