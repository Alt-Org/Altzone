using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskSymbolicQuestionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _logoImage;
    [Header("Questions")]
    [SerializeField] private Image _question1Image;
    [SerializeField] private Image _question1CheckmarkImage;
    [SerializeField] private TMP_Text _question1Text;
    [SerializeField] private Button _question1Button;
    [SerializeField] private Image _question2Image;
    [SerializeField] private Image _question2CheckmarkImage;
    [SerializeField] private TMP_Text _question2Text;
    [SerializeField] private Button _question2Button;
    [SerializeField] private Image _question3Image;
    [SerializeField] private Image _question3CheckmarkImage;
    [SerializeField] private TMP_Text _question3Text;
    [SerializeField] private Button _question3Button;
    [SerializeField] private Color _unselectedQuestionColour;
    [SerializeField] private Color _selectedQuestionColour;
    [Header("Answer Note")]
    [SerializeField] private Image _answerNoteImage;
    [SerializeField] private TMP_Text _answerNoteLabel;
    [SerializeField] private string _answerNoteBaseTextFI;
    [SerializeField] private string _answerNoteBaseTextEN;
    [SerializeField] private string _answerNoteCorrectTextFI;
    [SerializeField] private string _answerNoteCorrectTextEN;
    [SerializeField] private string _answerNoteIncorrectTextFI;
    [SerializeField] private string _answerNoteIncorrectTextEN;
    [SerializeField] private Color _baseAnswerNoteColour;
    [SerializeField] private Color _incorrectAnswerNoteColour;
    [SerializeField] private Color _correctAnswerNoteColour;

    private int _answer = 0;
    private bool _answered = false;

    private string AnswerNoteBaseText
    {
        get
        {
            if (Localization.Language is LanguageType.Finnish)
                return _answerNoteBaseTextFI;
            if (Localization.Language is LanguageType.English)
                return _answerNoteBaseTextEN;
            else
                return _answerNoteBaseTextFI;
        }
    }

    private string AnswerNoteCorrectText
    {
        get
        {
            if (Localization.Language is LanguageType.Finnish)
                return _answerNoteCorrectTextFI;
            if (Localization.Language is LanguageType.English)
                return _answerNoteCorrectTextEN;
            else
                return _answerNoteCorrectTextFI;
        }
    }

    private string AnswerNoteIncorrectText
    {
        get
        {
            if (Localization.Language is LanguageType.Finnish)
                return _answerNoteIncorrectTextFI;
            if (Localization.Language is LanguageType.English)
                return _answerNoteIncorrectTextEN;
            else
                return _answerNoteIncorrectTextFI;
        }
    }


    private void Start()
    {
        DailyTaskRecogniseSymbolic.OnActivateSymbolicQuestion += OpenQuestion;
    }

    private void OpenQuestion(Sprite logo, string option1, string option2, string option3, int answer)
    {
        if (_panel.activeSelf) return;

        _logoImage.sprite = logo;
        _question1Text.text = option1;
        _question2Text.text = option2;
        _question3Text.text = option3;
        _answer = answer;
        _answered = false;

        _question1Image.color = _unselectedQuestionColour;
        _question1CheckmarkImage.gameObject.SetActive(false);
        _question2Image.color = _unselectedQuestionColour;
        _question2CheckmarkImage.gameObject.SetActive(false);
        _question3Image.color = _unselectedQuestionColour;
        _question3CheckmarkImage.gameObject.SetActive(false);
        _answerNoteImage.color = _baseAnswerNoteColour;
        _answerNoteLabel.text = AnswerNoteBaseText;

        _question1Button.onClick.AddListener(() => CheckAnswer(1));
        _question2Button.onClick.AddListener(() => CheckAnswer(2));
        _question3Button.onClick.AddListener(() => CheckAnswer(3));

        _panel.SetActive(true);
    }

    private void CheckAnswer(int questionValue)
    {
        if (_answered) return;

        bool correct;

        if (questionValue == _answer)
        {
            correct = true;
            _answerNoteImage.color = _correctAnswerNoteColour;
            _answerNoteLabel.text = AnswerNoteCorrectText;
            if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
                    && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationCategory is EducationCategoryType.Story
                && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationStoryType is TaskEducationStoryType.Press3SymbolFurniture)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationStoryType.Press3SymbolFurniture, "1");
            }
        }
        else
        {
            correct = false;
            _answerNoteImage.color = _incorrectAnswerNoteColour;
            _answerNoteLabel.text = AnswerNoteIncorrectText;
        }

        switch (questionValue)
        {
            case 1:
                _question1Image.color = _selectedQuestionColour;
                _question1CheckmarkImage.gameObject.SetActive(correct);
                break;
            case 2:
                _question2Image.color = _selectedQuestionColour;
                _question2CheckmarkImage.gameObject.SetActive(correct);
                break;
            case 3:
                _question3Image.color = _selectedQuestionColour;
                _question3CheckmarkImage.gameObject.SetActive(correct);
                break;
        }
        _answered = true;

        

        StartCoroutine(CloseQuestion());
    }

    private IEnumerator CloseQuestion()
    {
        yield return new WaitForSeconds(2);
        _question1Button.onClick.RemoveAllListeners();
        _question2Button.onClick.RemoveAllListeners();
        _question3Button.onClick.RemoveAllListeners();
        _panel.SetActive(false);
    }
}
