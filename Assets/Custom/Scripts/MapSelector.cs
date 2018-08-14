using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Geocoding;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Examples;

public class MapSelector : MonoBehaviour
{
    public AbstractMap map;
    public Texture2D cursorTex;
    public CursorMode cursorMode = CursorMode.Auto;
    public GameObject panel;
    private Vector2 hotSpot;

    private bool isSelectingMapPoint;
    private Image buttonImage;
    private Text buttonText;

    public delegate void SelectPoint(Vector2d latlong);
    public SelectPoint selectCallback;

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

    private void OnEnable()
    {
        buttonImage.enabled = true;
        buttonText.enabled = true;
    }

    public void OnClickSelectMapButton()
    {
        Cursor.SetCursor(cursorTex, hotSpot, cursorMode);
        isSelectingMapPoint = true;
        buttonImage.enabled = false;
        buttonText.enabled = false;

        Camera.main.GetComponent<CameraMovement>().SetCanMove(true);

        onClickButtonCallback();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isSelectingMapPoint && Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            Camera.main.GetComponent<CameraMovement>().SetCanMove(false);

            Vector3 clickPos = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(clickPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                selectCallback(map.WorldToGeoPosition(hit.point));
                isSelectingMapPoint = false;
            }
            //else
            //    panel.SetActive(true); //missed so just go back
        }
    }


    //IEnumerator QueryPoints(Vector3 point)
    //{
    //    Vector2d vecLatLong = map.WorldToGeoPosition(point);
    //    string latLong = vecLatLong.y + "," + vecLatLong.x + ".json?";

    //    //string parameters = "types=poi&limit=5&";
    //    string parameters = "types=poi";

    //    string postURL = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + latLong + parameters + "&" + accessToken;


    //    WWW www = new WWW(postURL);
    //    yield return www;

    //    if (www.error != null)
    //    {
    //        Debug.Log("QueryAdd www error: " + www.error);
    //    }
    //    else
    //    {
    //        var response = MapboxAccess.Instance.Geocoder.Deserialize<ForwardGeocodeResponse>(www.text);
    //        Debug.Log("response: " + www.text);
    //        string district = "";
    //        foreach (Feature feature in response.Features)
    //        {
    //            district = feature.Context[0]["text"];
    //            Debug.Log("District" + district);
    //            break;
    //        }

    //        selectDistrictHandler(district, vecLatLong);
    //    }
    //}

}
