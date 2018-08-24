using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string id;
    public string text;
    public string type;
    public List<string> answers;
    public string category;
}

[System.Serializable]
public class RootObject
{
    public List<string> category;
    public List<Question> questions;
}