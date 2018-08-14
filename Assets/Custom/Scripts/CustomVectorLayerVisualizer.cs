using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.VectorTile;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.MeshGeneration.Filters;
using Mapbox.Unity.MeshGeneration.Interfaces;
using Mapbox.Unity.MeshGeneration.Components;

public class CustomVectorLayerVisualizer : VectorLayerVisualizer
{
    public static CustomFeatureBehaviourModifier featureModifier;

    public static List<CustomFeatureBehaviour> features
    { 
        get
        {
            if(featureModifier != null)
                return featureModifier.featureList;
            Debug.Log("Feature Modifier is null");
            return null;
        }
    }


    public void SetProperties(VectorSubLayerProperties properties, LayerPerformanceOptions performanceOptions)
    {
        base.SetProperties(properties, performanceOptions);
        //TODO stop hard coding it
        if (properties.coreOptions.layerName == "MyTileSet")
        {
            foreach(var modifier in properties.GoModifiers)
            {
                if(typeof(CustomFeatureBehaviourModifier) == modifier.GetType())
                {
                    featureModifier = (CustomFeatureBehaviourModifier)modifier;
                }
            }
        }        
    }




}
