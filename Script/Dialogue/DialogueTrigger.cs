using UnityEngine;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private string[] dialogueTexts; // Mảng chứa các đoạn hội thoại
    [SerializeField] private float dialogueSpeed = 0.05f; // Tốc độ hiển thị chữ
    [SerializeField] private float dialogueDelay = 2f; // Thời gian chờ giữa các đoạn hội thoại

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // Panel chứa hộp thoại
    [SerializeField] private TextMeshProUGUI dialogueText; // Text hiển thị nội dung

    private int currentDialogueIndex = -1; // Bắt đầu từ -1 để ShowNextDialogue() đầu tiên sẽ hiển thị index 0
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentText = "";
    private float typingTimer = 0f;
    private float delayTimer = 0f;
    private Animator amr;

    private void Start()
    {
        // Ẩn UI ban đầu
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        amr = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isDialogueActive)
        {
            StartDialogue();
            amr.SetBool("Talk", true);
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (isTyping)
        {
            // Xử lý hiển thị text từng chữ
            typingTimer += Time.deltaTime;
            if (typingTimer >= dialogueSpeed)
            {
                typingTimer = 0f;
                if (currentText.Length < dialogueTexts[currentDialogueIndex].Length)
                {
                    currentText += dialogueTexts[currentDialogueIndex][currentText.Length];
                    dialogueText.text = currentText;
                }
                else
                {
                    isTyping = false;
                    delayTimer = 0f;
                }
            }
        }
        else
        {
            // Đợi một khoảng thời gian trước khi chuyển đoạn tiếp theo
            delayTimer += Time.deltaTime;
            if (delayTimer >= dialogueDelay)
            {
                MoveToNextDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        if (dialogueTexts == null || dialogueTexts.Length == 0)
        {
            Debug.LogWarning("Không có nội dung hội thoại nào được thiết lập!");
            return;
        }

        isDialogueActive = true;
        currentDialogueIndex = -1; // Reset về -1
        
        // Hiển thị panel hội thoại
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        
        MoveToNextDialogue();
    }

    private void MoveToNextDialogue()
    {
        currentDialogueIndex++;
        if (currentDialogueIndex >= dialogueTexts.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentDialogue();
    }

    private void ShowCurrentDialogue()
    {
        currentText = "";
        dialogueText.text = "";
        isTyping = true;
        typingTimer = 0f;
        delayTimer = 0f;
    }

    private void EndDialogue()
    {
        amr.SetBool("Talk", false);
        isDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EndDialogue();
            amr.SetBool("Talk", false);
        }
    }
} 