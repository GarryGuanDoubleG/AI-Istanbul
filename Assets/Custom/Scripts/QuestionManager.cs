using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;

public class QuestionManager : MonoBehaviour {

    public Text questionText;
    public DistrictSelectionHandler districtHandler;
    public Dropdown answerDropdown;
    public InputField answerField;
    public MapSelector mapSelector;    

    private Dictionary<string, GameObject> displayDict;
    private GameObject currDisplay;
    private Transform panel;
    
    //text files
    public TextAsset questionsJson;    
    private RootObject questionData;

    //question containers
    private Dictionary<string, List<Question>> questionListByCategory; //quesiton list by category
    private List<Question> currentQuestionList;
    private int currentQuestionIndex;

    public Button skipButton;
    public Button categoryButton;

    private void Awake()
    {
        panel = gameObject.transform.GetChild(0);
        displayDict = new Dictionary<string, GameObject>();
        for(int i = 0; i < panel.childCount; i++)
        {
            var child = panel.GetChild(i).gameObject;
            var name = child.name;

            if (name == "Question Background")
            {
                questionText = child.GetComponentInChildren<Text>();
                displayDict.Add("question", child);
            }
            else if (name == "Category Question")
            {
                displayDict.Add("category", child);
            }
            else if (name == "Dropdown Question")
            {
                displayDict.Add("dropdown", child);
                answerDropdown = child.GetComponentInChildren<Dropdown>();
                answerDropdown.onValueChanged.AddListener(OnSelectDropdown);
            }
            else if (name == "Map Question")
            {
                displayDict.Add("map", child);
                mapSelector = child.GetComponent<MapSelector>();
                mapSelector.selectCallback = OnSelectMapPoint;
                mapSelector.onClickButtonCallback = OnSelectMapButton;
            }
            else if (name == "String Question")
            {
                displayDict.Add("input", child);
                answerField = child.GetComponentInChildren<InputField>();
                answerField.onEndEdit.AddListener(OnAnswerInputField);
            }
            else if (name == "Skip Button")
            {
                skipButton = child.GetComponent<Button>();
                skipButton.onClick.AddListener(OnSkipButton);
            }
        }
    }

    // Use this for initialization
    void Start ()
    {
        questionData = JsonUtility.FromJson<RootObject>(questionsJson.text);

        questionListByCategory = new Dictionary<string, List<Question>>();
        foreach (string category in questionData.category)
            questionListByCategory.Add(category, new List<Question>());

        foreach (Question question in questionData.questions)
            questionListByCategory[question.category].Add(question);

        InitCategoryQuestions();
    }
	
    void InitCategoryQuestions()
    {
        GameObject categoryObj = displayDict["category"].gameObject;
        currDisplay = categoryObj;

        var categoryRectTrans = categoryObj.GetComponent<RectTransform>();
        questionText.text = "Which categories do you want to answer";

        foreach (string category in questionData.category)
        {
            var button = Instantiate(categoryButton);
            button.transform.SetParent(categoryObj.transform, false);
            button.GetComponentInChildren<Text>().text = category;

            var clickHandler = button.GetComponent<CategoryButton>();
            clickHandler.text = category;
            clickHandler.callback = OnSelectCategory;
        }        
    }


    private void SetQuestionText(string text)
    {
        questionText.text = text;
    }

    private void SetNewDisplay(string displayName)
    {
        currDisplay.SetActive(false);

        //set new display
        currDisplay = displayDict[displayName];
        currDisplay.SetActive(true);        
    }

    private void SetMapQuestion(Question question)
    {
        SetNewDisplay("map");

        mapSelector.gameObject.SetActive(true);
        SetQuestionText(question.text);
    }

    private void SetDropdownQuestion(Question question)
    {
        SetNewDisplay("dropdown");

        answerDropdown.gameObject.SetActive(true);
        answerDropdown.ClearOptions();
        answerDropdown.AddOptions(question.answers);
        SetQuestionText(question.text);
    }

    private void SetInputQuestion(Question question)
    {
        SetNewDisplay("input");

        answerField.gameObject.SetActive(true);
        SetQuestionText(question.text);
    }

    private void SetQuestion(int index)
    {
        if(currentQuestionList == null)
            Debug.LogError("Question List is null");

        var question = currentQuestionList[index];
        if (question.type == "dropdown")
            SetDropdownQuestion(question);
        else if (question.type == "map")
            SetMapQuestion(question);
        else if (question.type == "input")
            SetInputQuestion(question);
    }

    private void SetNextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < currentQuestionList.Count)
            SetQuestion(currentQuestionIndex);
        else
        {
            questionText.text = "Select a category";
            SetNewDisplay("category");
        }
    }

    private void SetQuestionList(string category)
    {
        if (!questionListByCategory.ContainsKey(category))
            Debug.LogError("Question Key '" + category + "' does not exist");
        else
            currentQuestionList = questionListByCategory[category];

        currentQuestionIndex = 0;
        SetQuestion(currentQuestionIndex);
    }

    private void OnSelectMapButton()
    {
        panel.GetComponent<Image>().enabled = false;
        displayDict["question"].SetActive(false);
    }

    private void OnSelectMapPoint(Vector2d latlong)
    {
        Debug.Log("Lat long " + latlong);
        panel.GetComponent<Image>().enabled = true;
        displayDict["question"].SetActive(true);

        SetNextQuestion();
    }

    private void OnAnswerInputField(string answer)
    {
        Debug.Log("answer is " + answer);
        SetNextQuestion();
    }

    private void OnSelectDropdown(int index)
    {
        Debug.Log("answer is " + answerDropdown.options[index]);
        SetNextQuestion();
    }

    public void OnSelectCategory(string text)
    {
        GameObject categoryObj = displayDict["category"].gameObject;
        categoryObj.SetActive(false);        

        SetQuestionList(text);
    }

    public void OnSkipButton()
    {
        SetNextQuestion();
    }

	// Update is called once per frame
	void Update () {
		
	}

    void GetQuestions(string category)
    {

    }

}
