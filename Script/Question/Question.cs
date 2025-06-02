using UnityEngine;


[CreateAssetMenu(fileName = "New_Question", menuName = "Question ")] 
public class Question : ScriptableObject
{
    public string questionText; // câu hỏi
    public string Answer;      // Giai thich cau tra loi
    public string[] answers;    // đáp án
    public int correctAnswerIndex; // giá trị đáp án đúng trong array answers
}
