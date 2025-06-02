using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instans; 
    public List<Question> question;  // Danh sách công khai chứa tất cả các câu hỏi (Scriptable Objects).
    private List<Question> useQuestion = new List<Question>(); //Những câu hỏi đã được sử dụng


    private void Awake()
    {
        if(instans == null)
        {
            instans = this;
        }    
        
    }

    public Question GetRamdomQuestion()
    {
        // Tạo một bản sao của danh sách questions để lọc các câu hỏi chưa sử dụng.
        List<Question> availableQuestions = new List<Question>(question);

        // Loại bỏ các câu hỏi đã được sử dụng khỏi danh sách availableQuestions.
        availableQuestions.RemoveAll(q => useQuestion.Contains(q));

        if (availableQuestions.Count > 0)
        {
            Question selectedQuestion = availableQuestions[Random.Range(0, availableQuestions.Count)];

            useQuestion.Add(selectedQuestion);
            return selectedQuestion;
        }


        else
        {
            // Xóa danh sách usedQuestions để cho phép lặp lại các câu hỏi.
            useQuestion.Clear();    
            // Gọi đệ quy lại chính phương thức GetRandomQuestion để lấy câu hỏi sau khi reset list usedQuestions.
            return GetRamdomQuestion();
        }
    }   
}
