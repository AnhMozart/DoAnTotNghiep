using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Chest : MonoBehaviour
{
    private Animator amr;

    public GameObject questionUI;
    public Text questionText;
    public Button[] answerButtons;
    private bool checkAnswer; // kiểm tra xem đáp án đúng không

    [Header("Answer")]
    public GameObject AnswerUI;
    public Text Answer_Text;
    public String[] praiseText; //các lời khen ngợi
    public String[] mobilizeText; // các lời động viên
    public Text AnswerText;
    public Text correctAnswerText;
    public Button AnswerBtn;


    [SerializeField] private GameObject PlayGame_UI;


    //private bool playerInRange = false;
    private bool isOpened = false; // Biến để kiểm tra xem rương đã được mở hay chưa.
    private bool answered = false; // Đánh dấu xem đã trả lời chưa


    private void Start()
    {
        amr = GetComponent<Animator>();
        questionUI.SetActive(false);
        AnswerUI.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isOpened)
        {
            amr.SetTrigger("Open");
            SoundManager.instance.OpenChest();
            OnInteract();
            //playerInRange = true;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            amr.SetTrigger("Close");
            //playerInRange = false;
        }
    }


    /*void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened) // Kiểm tra thêm điều kiện rương chưa được mở.
        {
            OnInteract();
        }
    }*/

    public void OnInteract()
    {
        Question question = QuestionManager.instans.GetRamdomQuestion();
        if (question != null)
        {
            StartCoroutine(DelayDisplayQuestion(question));
            isOpened = true; // Đánh dấu rương đã được mở.
        }
    }


    IEnumerator DelayDisplayQuestion(Question _question)
    {
        yield return new WaitForSeconds(1f);
        DisplayQuestion(_question);
    }


    void DisplayQuestion(Question question)
    {
        questionText.text = question.questionText;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<Text>().text = question.answers[i];
            int answerIndex = i;
            //Xóa tất cả các listener sự kiện onClick hiện tại của button
            answerButtons[i].onClick.RemoveAllListeners();
            // 6. Thêm một listener sự kiện onClick mới cho button
            answerButtons[i].onClick.AddListener(() => CheckAnswer(question, answerIndex));
        }
        questionUI.SetActive(true);
        PlayGame_UI.SetActive(false);
        Time.timeScale = 0f;
    }


    void CheckAnswer(Question question, int answerIndex)
    {
        if (answered) return;

        answered = true;

        if (answerIndex == question.correctAnswerIndex)
        {
            SoundManager.instance.TraloiDung();
            PlayerManager.instance.AddCoin(100);
            StartCoroutine(FlashButton(answerButtons[answerIndex], Color.green, question));
            Time.timeScale = 1f;
            checkAnswer = true;
            Debug.Log("Correct!");
        }
        else
        {
            SoundManager.instance.TraloiSai();
            StartCoroutine(FlashButton(answerButtons[answerIndex], Color.red, question));
            PlayerManager.instance.MinusCoin(100);
            Debug.Log("Incorrect!");
            checkAnswer = false;
            Time.timeScale = 1f;
        }
    }


    IEnumerator FlashButton(Button button, Color color, Question question)
    {
        Color originalColor = button.image.color;
        button.image.color = color;
        yield return new WaitForSeconds(0.5f);
        button.image.color = originalColor;
        questionUI.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        ShowAnswer(question);
    }


    #region AnswerUI
  


    private void ShowAnswer(Question question)
    {
        int number = UnityEngine.Random.Range(0, 5);
        AnswerUI.SetActive(true);

        if(checkAnswer)
        {
            Answer_Text.text = praiseText[number].ToString();
        }    

        else
        {
            Answer_Text.text = mobilizeText[number].ToString();
        }

        if (string.IsNullOrEmpty(question.Answer))
        {
            correctAnswerText.text = "Câu trả lời: " + question.answers[question.correctAnswerIndex].ToString();
            AnswerText.text = "Không có giải thích cho câu trả lời này.";
        }
        else
        {
            correctAnswerText.text = "Câu trả lời: " + question.answers[question.correctAnswerIndex].ToString();
            AnswerText.text = "Giải thích: " + question.Answer;
        }
        AnswerBtn.onClick.RemoveAllListeners();
        AnswerBtn.onClick.AddListener(() => ShowAnswer());
    }    


    private void ShowAnswer()
    {
        Time.timeScale = 1f;
        AnswerUI.SetActive(false);
        PlayGame_UI.SetActive(true);
    }
    #endregion
}
 