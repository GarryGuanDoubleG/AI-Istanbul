namespace Mapbox.Unity.MeshGeneration.Components
{
	using UnityEngine;
	using System.Linq;
	using Mapbox.Unity.MeshGeneration.Data;
	using System;
    using Mapbox.Examples;


    public class CustomFeatureBehaviour : MonoBehaviour
	{
		public VectorEntity VectorEntity;
		public Transform Transform;
		public VectorFeatureUnity Data;

		[Multiline(5)]
		public string DataString;

		public void ShowDebugData()
		{
			DataString = string.Join("\r\n", Data.Properties.Select(x => x.Key + " - " + x.Value.ToString()).ToArray());
		}

		public void ShowDataPoints()
		{
			foreach (var item in VectorEntity.Feature.Points)
			{
				for (int i = 0; i < item.Count; i++)
				{
					var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					go.name = i.ToString();
					go.transform.SetParent(transform, false);
					go.transform.localPosition = item[i];
				}
			}
		}

		public void Initialize(VectorEntity ve)
		{
			VectorEntity = ve;
			Transform = transform;
			Data = ve.Feature;

            //var label = ve.GameObject.GetComponent<CustomLabelTextSetter>();
            //label.SetName(Data.Properties["name"].ToString());
        }

		public void Initialize(VectorFeatureUnity feature)
		{
			Transform = transform;
			Data = feature;
            //GetComponent<CustomLabelTextSetter>().SetName(Data.Properties["name"].ToString());
        }

        void Update()
        {
            //transform.Rotate(new Vector3(0, 90.0f * Time.deltaTime, 0.0f));
        }
	}
}