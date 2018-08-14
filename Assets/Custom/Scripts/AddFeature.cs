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

public class AddFeature : MonoBehaviour {

    Button addButton;
    AbstractMap map;

    public Texture2D cursorTex;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;

    public GameObject prefab;

    public enum SelectionMode
    {
        None,
        Location,
        Feature
    };

    public struct SelectionEntry
    {
        public GameObject gameObject;
        public Feature feature;

        public SelectionEntry(GameObject obj, Feature feature)
        {
            this.feature = feature;
            this.gameObject = obj;
        }
    }

    public List<SelectionEntry> selectionList;

    private SelectionMode selectionMode;

    private string accessToken = "access_token=pk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjampweTJjbGs4MmRjM2twMTdzbWl6dTkyIn0.nHcaY9wzvR1jyEgAk81fjg";
    private string writeToken = "access_token=sk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjamtkNmExbnkwNG56M3FwOGxlc2E0dG1sIn0.tZUP6QL4TP2ZVHr02RwhIw";

    private void Awake()
    {
        addButton = GetComponent<Button>();
        addButton.onClick.AddListener(HandleAddButton);

        map = GameObject.Find("Map").GetComponent<AbstractMap>();
    }
    // Use this for initialization
    void Start () {
        selectionList = new List<SelectionEntry>(5);
        hotSpot = new Vector2(cursorTex.width * .5f, cursorTex.height);
	}

    void SelectLocation()
    {
        Vector3 clickPos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(clickPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            SetSelectionMode(SelectionMode.Feature);
            StartCoroutine(QueryPoints(hit.point));
        }
        else
        {
            SetSelectionMode(SelectionMode.None);
        }
    }

    void SelectFeature()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitEntry = false;
        if(Physics.Raycast(ray, out hit))
        {
            foreach (var entry in selectionList)
            {
                if(entry.gameObject == hit.collider.gameObject)
                {
                    //send query to update dataset
                    StartCoroutine(InsertFeatureToDataSet(entry));
                    hitEntry = true;
                }
            }
        }

        if(hitEntry)
            SetSelectionMode(SelectionMode.None);
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))//left click
        {
            switch(selectionMode)
            {
                case SelectionMode.Location:
                    SelectLocation();
                    break;
                case SelectionMode.Feature:
                    SelectFeature();
                    break;
                default:
                    break;
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
            SetSelectionMode(SelectionMode.None);
    }

    void SetSelectionMode(SelectionMode newMode)
    {
        switch (selectionMode)
        {
            case SelectionMode.Location:
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                break;
            case SelectionMode.Feature:
                foreach (var entry in selectionList)
                    Destroy(entry.gameObject);

                selectionList.Clear();
                break;
            default:
                break;
        }


        switch (newMode)
        {
            case SelectionMode.Location:
                Cursor.SetCursor(cursorTex, hotSpot, cursorMode);
                break;
            default:
                break;
        }


        selectionMode = newMode;
    }

    void HandleAddButton()
    {
        SetSelectionMode(SelectionMode.Location);
    }

    IEnumerator QueryPoints(Vector3 point)
    {
        Vector2d vecLatLong = map.WorldToGeoPosition(point);
        string latLong = vecLatLong.y + "," + vecLatLong.x + ".json?";
        Debug.Log("lat long " + vecLatLong);

        //string parameters = "types=poi&limit=5&";
        string parameters = "types=poi&limit=5";

        string postURL = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + latLong  + parameters + "&" + accessToken;

        WWW www = new WWW(postURL);
        yield return www;        

        if (www.error != null)
        {
            Debug.Log("QueryAdd www error: " + www.error);
        }
        else
        {
            var response = MapboxAccess.Instance.Geocoder.Deserialize<ForwardGeocodeResponse>(www.text);

            foreach(Feature feature in response.Features)
            {
                Vector3 spawnPoint = map.GeoToWorldPosition(feature.Center);
                spawnPoint.y += prefab.GetComponent<BoxCollider>().size.y * prefab.transform.localScale.y * .5f;

                var selectGO = Instantiate(prefab, spawnPoint, Quaternion.identity);
                var label = selectGO.GetComponent<CustomLabelTextSetter>();
                label.SetName(feature.PlaceName);

                var entry = new SelectionEntry(selectGO, feature);
                selectionList.Add(entry);
            }
        }
    }

    IEnumerator InsertFeatureToDataSet(SelectionEntry entry)
    {
        Feature newFeature = entry.feature;
        if (!newFeature.Properties.ContainsKey("district"))
        {
            newFeature.Properties.Add("district", newFeature.Context[0]["text"]);
            newFeature.Properties.Add("name", newFeature.Text);
        }

        //string tilesetId = "cjjxgjqlw015wksmet7qq0ip1-8ngzu";
        string myId = "amykhoover";
        string datasetId = "cjjxgjqlw015wksmet7qq0ip1";
        string feature = Mapbox.Json.JsonConvert.SerializeObject(newFeature);

        //"https://api.mapbox.com/datasets/v1/[myID]/[myDatasetID]/features/[myFeatureID]?access_token=[myAccessToken]";

        string postURL = string.Format(
            "https://api.mapbox.com/datasets/v1/{0}/{1}/features/{2}?{3}"
            , myId
            , datasetId
            , entry.feature.Id
            , writeToken
        );

        //string postURL = "https://api.mapbox.com/datasets/v1/";


        //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(feature);
        // initialize as PUT
        UnityWebRequest www = UnityWebRequest.Put(postURL, feature);
        www.SetRequestHeader("Content-Type", "application/json");
        // HACK!!! override method and convert to POST
        www.method = "PUT";

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("ApplyDatasetToTileset www error: " + www.error);
        }
        else
        {
            Debug.Log("ApplyDatasetToTileset www = " + www.downloadHandler.text);
            StartCoroutine(ApplyDatasetToTileset());
        }

        yield return null;
    }

    IEnumerator ApplyDatasetToTileset()
    {
        string myId = "amykhoover";
        string datasetId = "cjjxgjqlw015wksmet7qq0ip1";
        string tilesetId = "cjjxgjqlw015wksmet7qq0ip1-8ngzu";

        string postURL = string.Format(
            "https://api.mapbox.com/uploads/v1/{0}?{1}"
            , myId
            , writeToken
        );

        string datasetTilesetInfo = string.Format(
            @"{{""name"":""MyTileSet"",""tileset"":""{0}.{1}"",""url"":""mapbox://datasets/{0}/{2}""}}"
            , myId
            , tilesetId
            , datasetId
        );
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(datasetTilesetInfo);
        // initialize as PUT
        UnityWebRequest www = UnityWebRequest.Put(postURL, bytes);
        www.SetRequestHeader("content-type", "application/json");
        // HACK!!! override method and convert to POST
        www.method = "POST";

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("ApplyDatasetToTileset www error: " + www.error);
        }
        else
        {
            Debug.Log("ApplyDatasetToTileset www = " + www.downloadHandler.text);
            MapboxAccess.Instance.ClearAllCacheFiles();
        }
    }
}
