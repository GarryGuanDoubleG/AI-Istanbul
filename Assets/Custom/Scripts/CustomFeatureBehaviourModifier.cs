namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Components;
	using UnityEngine;
	using System.Collections.Generic;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Add Feature Behaviour Modifier")]
	public class CustomFeatureBehaviourModifier : GameObjectModifier
	{
		private Dictionary<GameObject, CustomFeatureBehaviour> _features;        
		private CustomFeatureBehaviour _tempFeature;

        public List<CustomFeatureBehaviour> featureList;

        //TODO make this a dictionary for GO lookups
        public GameObject prefab;

        public string searchFilter;

        public void OnSearchFilter(string filter)
        {
            searchFilter = filter;
            ApplyFilter();
        }

        void ApplyFilter()
        {
            foreach (CustomFeatureBehaviour feature in featureList)
            {
                ApplyFilter(feature);
            }
        }

        void ApplyFilter(CustomFeatureBehaviour feature)
        {
            //if (!feature.VectorEntity.Feature.Properties["name"].ToString().ToLower().Contains(searchFilter.ToLower()))
            //{
            //    feature.gameObject.SetActive(false);
            //}
            //else
            //{
            //    feature.gameObject.SetActive(true);
            //}
        }

		public override void Initialize()
		{
			if (_features == null)
			{
				_features = new Dictionary<GameObject, CustomFeatureBehaviour>();
			}

            featureList.Clear();
		}

		public override void Run(VectorEntity ve, UnityTile tile)
		{
            Vector3 posOffset = new Vector3(0, prefab.GetComponent<BoxCollider>().size.y, 0);
            posOffset.Scale(prefab.transform.localScale * .5f);

            ve.GameObject.transform.position = ve.GameObject.transform.position + posOffset;

            if (_features.ContainsKey(ve.GameObject))
			{
                _features[ve.GameObject].Initialize(ve);
                ApplyFilter(_features[ve.GameObject]);
            }
			else
			{                             
                _tempFeature = ve.GameObject.AddComponent<CustomFeatureBehaviour>();
                _features.Add(ve.GameObject, _tempFeature);
				_tempFeature.Initialize(ve);

                ApplyFilter(_tempFeature);
                featureList.Add(_tempFeature);
			}
		}
	}
}