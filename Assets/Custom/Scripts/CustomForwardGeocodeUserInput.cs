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
    using Mapbox.Unity.MeshGeneration.Modifiers;

    [RequireComponent(typeof(InputField))]
	public class CustomForwardGeocodeUserInput : ForwardGeocodeUserInput
	{
		//InputField _inputField;

		//ForwardGeocodeResource _resource;

		//Vector2d _coordinate;
		//public Vector2d Coordinate
		//{
		//	get
		//	{
		//		return _coordinate;
		//	}
		//}

		//bool _hasResponse;
		//public bool HasResponse
		//{
		//	get
		//	{
		//		return _hasResponse;
		//	}
		//}

		//public ForwardGeocodeResponse Response { get; private set; }

		////public event Action<> OnGeocoderResponse = delegate { };
		//public event Action<ForwardGeocodeResponse> OnGeocoderResponse = delegate { };

        public delegate void OnSearchFilter(string filter);
        OnSearchFilter searchCallback;

		void Awake()
		{
			//_inputField = GetComponent<InputField>();
			//_inputField.onValueChanged.AddListener(HandleUserInput);
   //         _resource = new ForwardGeocodeResource("");

        }

        private void Start()
        {
            //Mesh
            //searchCallback = 
        }


        void HandleUserInput(string searchString)
		{
            foreach (CustomFeatureBehaviour feature in CustomFeatureBehaviourModifier.featureList)
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
            base.HandleGeocoderResponse(res);
		}
	}
}
