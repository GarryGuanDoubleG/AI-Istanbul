namespace Mapbox.Examples
{
	using Mapbox.Unity.MeshGeneration.Interfaces;
	using System.Collections.Generic;
	using UnityEngine;

	public class CustomLabelTextSetter : MonoBehaviour, IFeaturePropertySettable
	{
		[SerializeField]
		TextMesh _textMesh; 

		public void Set(Dictionary<string, object> props)
		{
			_textMesh.text = "";

			if (props.ContainsKey("name"))
			{
				_textMesh.text = props["name"].ToString();
			}
			else if (props.ContainsKey("house_num"))
			{
				_textMesh.text = props["house_num"].ToString();
			}
			else if (props.ContainsKey("type"))
			{
				_textMesh.text = props["type"].ToString();
			}
		}

        public void SetName(string name)
        {
            _textMesh.text = name;
        }

        public void OnMouseEnter()
        {
            _textMesh.color = Color.red;
        }

        public void OnMouseExit()
        {
            _textMesh.color = Color.white;
        }

        void Update()
        {
            //float hypo = Vector3.Distance(Camera.main.transform.position, _textMesh.transform.position);
            //float adjacentDIst = _textMesh.transform.position.z - Camera.main.transform.position.z;

            //float angle = Mathf.Acos(adjacentDIst / hypo) * Mathf.Rad2Deg;
            //_textMesh.transform.rotation = Quaternion.Euler(Vector3.right * angle);

            //Vector3 v = Camera.main.transform.position - _textMesh.transform.position;
            ////v.x = v.z = 0.0f;
            //_textMesh.transform.LookAt(Camera.main.transform.position - v);
            ////transform.Rotate(0, 180, 0);
        }
    }
}