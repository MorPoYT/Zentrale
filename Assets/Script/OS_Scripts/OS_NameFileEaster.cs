using UnityEngine;
using TMPro;
using System;

public class OS_NameFileEaster : MonoBehaviour
{
    [Header("File Information")]
    [Tooltip("Full file name")]
    public string fileName = "example.txt";
    public string fileFormat;

    [Header("UI References")]
    [Tooltip("Text field for file name")]
    public TMP_Text fileNameText;

    // Format for displaying date and time
    private const string DateTimeFormat = "dd_MM_yyyy";

    void Start()
    {
        UpdateFileInfoUI();
    }

    public void UpdateFileInfoUI()
    {
    	string dateTimeStr = DateTime.Now.ToString(DateTimeFormat) + fileFormat;
        // Update file name
        if (fileNameText != null)
        {
            fileNameText.text = fileName + dateTimeStr;
        }
    }
}