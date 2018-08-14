//-----------------------------------------------------------------------
// <copyright file="ForwardGeocodeUserInput.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Examples
{
	using Mapbox.Unity;
	using UnityEngine;
	using UnityEngine.UI;
	using System;
	using Mapbox.Geocoding;
	using Mapbox.Utils;
    using Mapbox.Unity.MeshGeneration.Components;

    [RequireComponent(typeof(InputField))]
	public class CustomForwardGeocodeUserInput : ForwardGeocodeUserInput
	{
		InputField _inputField;

		ForwardGeocodeResource _resource;

		Vector2d _coordinate;
		public Vector2d Coordinate
		{
			get
			{
				return _coordinate;
			}
		}

		bool _hasResponse;
		public bool HasResponse
		{
			get
			{
				return _hasResponse;
			}
		}

		public ForwardGeocodeResponse Response { get; private set; }

		//public event Action<> OnGeocoderResponse = delegate { };
		public event Action<ForwardGeocodeResponse> OnGeocoderResponse = delegate { };

        public delegate void OnSearchFilter(string filter);
        OnSearchFilter searchCallback;

		void Awake()
		{
			_inputField = GetComponent<InputField>();
			_inputField.onValueChanged.AddListener(HandleUserInput);
            _resource = new ForwardGeocodeResource("");

        }

        private void Start()
        {
            //Mesh
            //searchCallback = 
        }


        void HandleUserInput(string searchString)
		{
            //_hasResponse = false;
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //	_resource.Query = searchString;
            //	MapboxAccess.Instance.Geocoder.Geocode(_resource, HandleGeocoderResponse);
            //}


            foreach (CustomFeatureBehaviour feature in CustomVectorLayerVisualizer.features)
            {
                if (feature.VectorEntity.Feature.Properties.ContainsKey("name"))
                {
                    if (!feature.VectorEntity.Feature.Properties["name"].ToString().ToLower().Contains(searchString.ToLower()))                    
                        feature.gameObject.SetActive(false);                   
                    else                    
                        feature.gameObject.SetActive(true);                    
                }
            }
        }

		void HandleGeocoderResponse(ForwardGeocodeResponse res)
		{
			_hasResponse = true;
			if (null == res)
			{
				_inputField.text = "no geocode response";
			}
			else if (null != res.Features && res.Features.Count > 0)
			{
				var center = res.Features[0].Center;
				//_inputField.text = string.Format("{0},{1}", center.x, center.y);
				_coordinate = res.Features[0].Center;
			}
			Response = res;
			OnGeocoderResponse(res);
		}
	}
}
