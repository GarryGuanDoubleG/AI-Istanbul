using Mapbox.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Geocoding;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Examples;

public class DistrictSelectionHandler : MonoBehaviour {

    public Dropdown dropdownUI;
    public GameObject canvasItems;
    private UIManager.OnSelectDistrictHandler selectDistrictHandler;

    public Texture2D cursorTex;
    public CursorMode cursorMode = CursorMode.Auto;

    public AbstractMap map;

    private Vector2 hotSpot;
    private bool isSelectingMapPoint;

    private List<string> districts = new List<string>
    {
        "Adalar",
        "Arnavutköy",
        "Ataşehir",
        "Avcılar",
        "Bağcılar",
        "Bahçelievler",
        "Bakırköy",
        "Başakşehir",
        "Bayrampaşa",
        "Beşiktaş",
        "Beykoz",
        "Beylikdüzü",
        "Beyoğlu",
        "Büyükçekmece",
        "Çatalca",
        "Çekmeköy",
        "Esenler",
        "Esenyurt",
        "Eyüp",
        "Fatih",
        "Gaziosmanpaşa",
        "Güngören",
        "Kadıköy",
        "Kağıthane",
        "Kartal",
        "Küçükçekmece",
        "Maltepe",
        "Pendik",
        "Sancaktepe",
        "Sarıyer",
        "Silivri",
        "Sultanbeyli",
        "Sultangazi",
        "Şile",
        "Şişli",
        "Tuzla",
        "Ümraniye",
        "Üsküdar",
        "Zeytinburnu",
    };
    private List<Mapbox.Utils.Vector2d> districtLongLat = new List<Mapbox.Utils.Vector2d>()
    {
        new Mapbox.Utils.Vector2d(40.866667, 29.1),
        new Mapbox.Utils.Vector2d(41.185556, 28.740556),
        new Mapbox.Utils.Vector2d(40.983333, 29.127778),
        new Mapbox.Utils.Vector2d(40.979167, 28.721389),
        new Mapbox.Utils.Vector2d(41.040556, 28.826111),
        new Mapbox.Utils.Vector2d(40.9975, 28.850556),
        new Mapbox.Utils.Vector2d(40.983056, 28.853611),
        new Mapbox.Utils.Vector2d(41.083333, 28.816667),
        new Mapbox.Utils.Vector2d(41.048056, 28.900278),
        new Mapbox.Utils.Vector2d(41.0425, 29.007222),
        new Mapbox.Utils.Vector2d(41.125, 29.088889),
        new Mapbox.Utils.Vector2d(40.966667, 28.711111),
        new Mapbox.Utils.Vector2d(41.036944, 28.9775),
        new Mapbox.Utils.Vector2d(41.02, 28.5775),
        new Mapbox.Utils.Vector2d(41.141667, 28.463056),
        new Mapbox.Utils.Vector2d(41.036944, 29.178611),
        new Mapbox.Utils.Vector2d(41.079444, 28.853889),
        new Mapbox.Utils.Vector2d(41.034281, 28.680119),
        new Mapbox.Utils.Vector2d(41.038889, 28.934722),
        new Mapbox.Utils.Vector2d(41.0225, 28.940833),
        new Mapbox.Utils.Vector2d(41.049167, 28.901389),
        new Mapbox.Utils.Vector2d(41.035, 28.8575),
        new Mapbox.Utils.Vector2d(40.991111, 29.026111),
        new Mapbox.Utils.Vector2d(41.071944, 28.966389),
        new Mapbox.Utils.Vector2d(40.910833, 29.161667),
        new Mapbox.Utils.Vector2d(41, 28.8),
        new Mapbox.Utils.Vector2d(40.925, 29.151667),
        new Mapbox.Utils.Vector2d(40.8775, 29.251389),
        new Mapbox.Utils.Vector2d(40.983333, 29.2),
        new Mapbox.Utils.Vector2d(41.191111, 29.009444),
        new Mapbox.Utils.Vector2d(41.083333, 28.25),
        new Mapbox.Utils.Vector2d(41.183333, 28.983333),
        new Mapbox.Utils.Vector2d(41.099167, 28.868056),
        new Mapbox.Utils.Vector2d(41.176389, 29.612778),
        new Mapbox.Utils.Vector2d(41.060278, 28.987778),
        new Mapbox.Utils.Vector2d(40.877778, 29.335556),
        new Mapbox.Utils.Vector2d(41.133056, 28.141667),
        new Mapbox.Utils.Vector2d(41.016667, 29.033333),
        new Mapbox.Utils.Vector2d(40.983056, 28.899722),
    };

    private string accessToken = "access_token=pk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjampweTJjbGs4MmRjM2twMTdzbWl6dTkyIn0.nHcaY9wzvR1jyEgAk81fjg";

    // Use this for initialization
    void Start () {
        selectDistrictHandler = UIManager.GetSelectDistrictHandler();

        hotSpot = new Vector2(cursorTex.width * .5f, cursorTex.height);

        dropdownUI.ClearOptions();
        dropdownUI.AddOptions(districts);
    }

    public void OnSelectDistrict()
    {
        string district = districts[dropdownUI.value];
        Mapbox.Utils.Vector2d longlat = districtLongLat[dropdownUI.value];
        //double lat = longlat.x;
        //double lng = longlat.y;
        //longlat = new Mapbox.Utils.Vector2d(lng, lat);

        Debug.Log("District " + district);
        Debug.Log("District longlat " + longlat);

        selectDistrictHandler(district, longlat);
    }
   

    public void OnClickSelectOnMapButton()
    {
        Cursor.SetCursor(cursorTex, hotSpot, cursorMode);
        canvasItems.SetActive(false);
        isSelectingMapPoint = true;
        Camera.main.GetComponent<CameraMovement>().SetCanMove(true);
    }

    // Update is called once per frame
    void Update () {
		if(isSelectingMapPoint && Input.GetMouseButtonDown(0))
        {
            isSelectingMapPoint = false;
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            Camera.main.GetComponent<CameraMovement>().SetCanMove(false);
            canvasItems.SetActive(true);

            Vector3 clickPos = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(clickPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f))
                StartCoroutine(QueryPoints(hit.point));
            else
                canvasItems.SetActive(true); //missed so just go back           
        }
	}

    IEnumerator QueryPoints(Vector3 point)
    {
        Vector2d vecLatLong = map.WorldToGeoPosition(point);
        string latLong = vecLatLong.y + "," + vecLatLong.x + ".json?";

        //string parameters = "types=poi&limit=5&";
        string parameters = "types=poi";

        string postURL = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + latLong + parameters + "&" + accessToken;


        WWW www = new WWW(postURL);
        yield return www;

        if (www.error != null)
        {
            Debug.Log("QueryAdd www error: " + www.error);
        }
        else
        {
            var response = MapboxAccess.Instance.Geocoder.Deserialize<ForwardGeocodeResponse>(www.text);
            Debug.Log("response: " + www.text);
            string district = "";
            foreach (Feature feature in response.Features)
            {
                district = feature.Context[0]["text"];
                Debug.Log("District" + district);
                break;
            }

            selectDistrictHandler(district, vecLatLong);
        }
    }

}
