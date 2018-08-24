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

	[RequireComponent(typeof(InputField))]
	public class ForwardGeocodeUserInput : MonoBehaviour
	{
		protected InputField _inputField;

		protected ForwardGeocodeResource _resource;

		protected Vector2d _coordinate;
		public Vector2d Coordinate
		{
			get
			{
				return _coordinate;
			}
		}

		protected bool _hasResponse;
		public bool HasResponse
		{
			get
			{
				return _hasResponse;
			}
		}

		public ForwardGeocodeResponse Response { get; protected set; }

		//public event Action<> OnGeocoderResponse = delegate { };
		public event Action<ForwardGeocodeResponse> OnGeocoderResponse = delegate { };

		void Awake()
		{
			_inputField = GetComponent<InputField>();
			_inputField.onEndEdit.AddListener(HandleUserInput);
			_resource = new ForwardGeocodeResource("");
		}

		void HandleUserInput(string searchString)
		{
			_hasResponse = false;
			if (!string.IsNullOrEmpty(searchString))
			{
				_resource.Query = searchString;
				MapboxAccess.Instance.Geocoder.Geocode(_resource, HandleGeocoderResponse);
			}
		}

		protected void HandleGeocoderResponse(ForwardGeocodeResponse res)
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
