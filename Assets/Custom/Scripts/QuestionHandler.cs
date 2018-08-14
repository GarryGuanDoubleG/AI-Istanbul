//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Mapbox.Map;

//public class QuestionHandler : MonoBehaviour {
//    private string accessToken = "access_token=pk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjampweTJjbGs4MmRjM2twMTdzbWl6dTkyIn0.nHcaY9wzvR1jyEgAk81fjg";
//    private string writeToken = "access_token=sk.eyJ1IjoiYW15a2hvb3ZlciIsImEiOiJjamtkNmExbnkwNG56M3FwOGxlc2E0dG1sIn0.tZUP6QL4TP2ZVHr02RwhIw";

//    Abstra

//    private void Awake()
//    {

//    }
//    // Use this for initialization
//    void Start()
//    {
//        selectionList = new List<SelectionEntry>(5);
//        hotSpot = new Vector2(cursorTex.width * .5f, cursorTex.height);
//    }

//    void SelectLocation()
//    {
//        int height = cursorTex.height;
//        int width = cursorTex.width;

//        Vector3 clickPos = Input.mousePosition;

//        Ray ray = Camera.main.ScreenPointToRay(clickPos);
//        RaycastHit hit;
//        if (Physics.Raycast(ray, out hit, 1000.0f))
//        {
//            SetSelectionMode(SelectionMode.Feature);
//            StartCoroutine(QueryPoints(hit.point));
//        }
//        else
//        {
//            SetSelectionMode(SelectionMode.None);
//        }
//    }


//    IEnumerator QueryPoints(Vector3 point)
//    {
//        Vector2d vecLatLong = map.WorldToGeoPosition(point);
//        string latLong = vecLatLong.y + "," + vecLatLong.x + ".json?";

//        //string parameters = "types=poi&limit=5&";
//        string parameters = "types=poi&limit=5";

//        string postURL = "https://api.mapbox.com/geocoding/v5/mapbox.places/" + latLong + parameters + "&" + accessToken;

//        WWW www = new WWW(postURL);
//        yield return www;

//        if (www.error != null)
//        {
//            Debug.Log("QueryAdd www error: " + www.error);
//        }
//        else
//        {
//            var response = MapboxAccess.Instance.Geocoder.Deserialize<ForwardGeocodeResponse>(www.text);

//            foreach (Feature feature in response.Features)
//            {
//                Vector3 spawnPoint = map.GeoToWorldPosition(feature.Center);
//                spawnPoint.y += prefab.GetComponent<BoxCollider>().size.y * prefab.transform.localScale.y * .5f;

//                var selectGO = Instantiate(prefab, spawnPoint, Quaternion.identity);
//                var label = selectGO.GetComponent<CustomLabelTextSetter>();
//                label.SetName(feature.PlaceName);

//                var entry = new SelectionEntry(selectGO, feature);
//                selectionList.Add(entry);
//            }
//        }
//    }


//}
