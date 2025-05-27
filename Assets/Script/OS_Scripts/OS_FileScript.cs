using UnityEngine;
using TMPro;
using System;

public class OS_FileScript : MonoBehaviour
{
    [Header("File Information")]
    [Tooltip("Full file name with extension (e.g. document.txt)")]
    public string fileName = "example.txt";

    [Tooltip("File size in bytes")]
    public long fileSize = 1024; // Default 1KB

    [Header("Date & Time Settings")]
    [Tooltip("If true, will use current system date/time")]
    public bool useSystemTime = true;

    [Tooltip("Custom date/time (used if Use System Time is false)")]
    public string customDateTime = "27.05.2025 22:30";

    [Header("UI References")]
    [Tooltip("Text field for file name")]
    public TMP_Text fileNameText;

    [Tooltip("Text field for date/time")]
    public TMP_Text dateTimeText;

    [Tooltip("Text field for file size")]
    public TMP_Text fileSizeText;

    // Format for displaying date and time
    private const string DateTimeFormat = "dd.MM.yyyy HH:mm";

    void Start()
    {
        UpdateFileInfoUI();
    }

    public void UpdateFileInfoUI()
    {
        // Update file name
        if (fileNameText != null)
        {
            fileNameText.text = fileName;
        }

        // Update date/time
        string dateTimeStr = useSystemTime 
            ? DateTime.Now.ToString(DateTimeFormat) 
            : customDateTime;

        if (dateTimeText != null)
        {
            dateTimeText.text = dateTimeStr;
        }

        // Update file size
        if (fileSizeText != null)
        {
            fileSizeText.text = FormatFileSize(fileSize);
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:0.##} {suffixes[suffixIndex]}";
    }

    // This method can be called from Unity Editor button
    [ContextMenu("Update File Info")]
    public void EditorUpdateFileInfo()
    {
        UpdateFileInfoUI();
    }
}