using System;
using TMPro;
using UnityEngine;

public class NoteWindow : MonoBehaviour
{
    public TMP_InputField noteInputField;
    [SerializeField] private TextMeshProUGUI metadataText;

    private PostItNote currentlyEditedNote;

    public void SaveNote()
    {
        currentlyEditedNote.Text = noteInputField.text;
        CloseWindow();
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
        currentlyEditedNote = null;
    }

    public void ShowWindow(PostItNote postItNote)
    {
        currentlyEditedNote = postItNote;
        metadataText.text = $"Datum: {DateTime.Now:dd.MM.yyyy}\nUsername: (Demo)";
        gameObject.SetActive(true);
        noteInputField.text = postItNote.Text;
    }
}