using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public TMP_Text questionText;

    private List<string> questions;
    private int currentIndex = 0;  // 현재 문제 번호

    void Start()
    {
        QuizDB db = new QuizDB();
        questions = db.LoadQuestions();

        if (questions.Count > 0)
        {
            ShowQuestion(currentIndex);
        }
        else
        {
            questionText.text = "문제가 없습니다.";
        }
    }

    public void NextQuestion()
    {
        if (currentIndex < questions.Count - 1)
        {
            currentIndex++;
            ShowQuestion(currentIndex);
        }
        else
        {
            Debug.Log("마지막 문제입니다.");
        }
    }

    public void PrevQuestion()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowQuestion(currentIndex);
        }
        else
        {
            Debug.Log("첫 번째 문제입니다.");
        }
    }

    private void ShowQuestion(int index)
    {
        questionText.text = questions[index];
        Debug.Log("현재 문제: " + questions[index]);
    }
}
