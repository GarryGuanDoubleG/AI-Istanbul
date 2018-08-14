namespace Mapbox.Examples
{
	using Mapbox.Geocoding;
	using UnityEngine.UI;
	using Mapbox.Unity.Map;
	using UnityEngine;
	using System;
	using System.Collections;

	public class ReloadMap : MonoBehaviour
	{
		Camera _camera;
		Vector3 _cameraStartPos;
		AbstractMap _map;

		[SerializeField]
		CustomForwardGeocodeUserInput _forwardGeocoder;

		[SerializeField]
		Slider _zoomSlider;

		Coroutine _reloadRoutine;

		WaitForSeconds _wait;

        [SerializeField]
        Text _LatLong;
		void Awake()
		{
			_camera = Camera.main;
			_cameraStartPos = _camera.transform.position;
			_map = FindObjectOfType<AbstractMap>();
			_forwardGeocoder.OnGeocoderResponse += ForwardGeocoder_OnGeocoderResponse;
            _zoomSlider.value = _map.Zoom;
			_zoomSlider.onValueChanged.AddListener(Reload);
			_wait = new WaitForSeconds(.3f);

            _LatLong.text = "Lat/Long: " + _map.CenterLatitudeLongitude.ToString();
		}

		void ForwardGeocoder_OnGeocoderResponse(ForwardGeocodeResponse response)
		{
			if(response == null)
			{
				return;
			}
			_camera.transform.position = _cameraStartPos;
			if (null != response.Features && response.Features.Count > 0)
			{
				_map.UpdateMap(response.Features[0].Center, (int)_zoomSlider.value);
			}
		}

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                var latlong = _map.WorldToGeoPosition(hit.point);
                _LatLong.text = "Lat/Long: " + latlong.x.ToString() + ", " + latlong.y.ToString();
            }                        
        }

		void Reload(float value)
		{
			if (_reloadRoutine != null)
			{
				StopCoroutine(_reloadRoutine);
				_reloadRoutine = null;
			}
			_reloadRoutine = StartCoroutine(ReloadAfterDelay((int)value));
		}

		IEnumerator ReloadAfterDelay(int zoom)
		{
			yield return _wait;
			_camera.transform.position = _cameraStartPos;
			_map.UpdateMap(_map.CenterLatitudeLongitude, zoom);
			_reloadRoutine = null;
		}
	}
}