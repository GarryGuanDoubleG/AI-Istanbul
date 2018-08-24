using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mapbox.Geocoding;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Examples;
using Mapbox.Unity.MeshGeneration.Components;

public class MapSelector : MonoBehaviour
{
    public AbstractMap map;
    public Texture2D cursorTex;
    public CursorMode cursorMode = CursorMode.Auto;
    public GameObject panel;

    //select map UI objects
    public GameObject selectFeature;
    public GameObject zoomSlider;
    public GameObject addFeatureButton;
    public GameObject searchFeature;

    private Vector2 hotSpot;

    private bool isAddingFeature;
    private bool isSelectingMapFeature;
    private Image buttonImage;
    private Text buttonText;

    public delegate void SelectFeature(string featureName, string featureLatLong);
    public SelectFeature selectCallback;

    public delegate void OnClickButton();
    public OnClickButton onClickButtonCallback;

    private string accessToken = "access_token=pk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjampweTJjbGs4MmRjM2twMTdzbWl6dTkyIn0.nHcaY9wzvR1jyEgAk81fjg";

    private void Awake()
    {
        if(panel == null)
        {
            Debug.LogError("Panel cannot be null");
        }
        buttonImage = transform.GetChild(0).GetComponent<Image>();
        buttonText = buttonImage.transform.GetChild(0).GetComponent<Text>();
    }
    private void Start()
    {
        selectFeature.SetActive(true);

        addFeatureButton.SetActive(false);
        zoomSlider.SetActive(false);
        searchFeature.SetActive(false);

        var addFeatureComponent = addFeatureButton.GetComponent<AddFeature>();
        addFeatureComponent.onClickAddButton = OnClickAddFeature;
        addFeatureComponent.addFeatureCallback = OnAddFeature;
    }

    private void OnEnable()
    {
    }

    public void ResetState()
    {
        buttonImage.enabled = true;
        buttonText.enabled = true;
        isSelectingMapFeature = false;
        isAddingFeature = false;

        addFeatureButton.SetActive(false);
        zoomSlider.SetActive(false);
        searchFeature.SetActive(false);

        Camera.main.GetComponent<CameraMovement>().SetCanMove(false);
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    public void OnClickAddFeature()
    {
        isAddingFeature = true;
    }
    public void OnAddFeature(string featureName, Vector2d featureLatLong)
    {
        selectCallback(featureName, featureLatLong.ToString());
        Debug.Log("Add feature name " + featureName);
        Debug.Log("Add feature latlong " + featureLatLong.ToString());

        ResetState();
    }

    public void OnClickSelectMapButton()
    {
        Cursor.SetCursor(cursorTex, hotSpot, cursorMode);
        //get rid of UI that are in the way
        isSelectingMapFeature = true;
        buttonImage.enabled = false;
        buttonText.enabled = false;
        panel.GetComponent<Image>().enabled = false;

        //enable options for selecting features
        addFeatureButton.SetActive(true);
        zoomSlider.SetActive(true);
        searchFeature.SetActive(true);

        Camera.main.GetComponent<CameraMovement>().SetCanMove(true);

        onClickButtonCallback();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (isSelectingMapFeature && !isAddingFeature && Input.GetMouseButtonDown(0))
        {                  
            Vector3 clickPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(clickPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                CustomFeatureBehaviour feature = hit.collider.gameObject.GetComponentInParent<CustomFeatureBehaviour>();
                if(feature != null)
                {
                    string name = feature.Data.Properties["name"].ToString();
                    Vector2d latlong = map.WorldToGeoPosition(hit.point);                    
                    selectCallback(name, latlong.ToString());

                    Debug.Log("Latlong " + latlong);
                    Debug.Log(name);

                    ResetState();
                }
            }
        }
    }
}
