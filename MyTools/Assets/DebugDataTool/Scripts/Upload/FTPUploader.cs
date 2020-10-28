using UnityEngine;
using System;
using System.Net;
using System.IO;

public class FTPUploader
{
    private string FTPHost = "http://writeFTPServer.here";
    private string FTPUserName = "username";
    private string FTPPassword = "password";

    public void UploadFile(string FilePath)
    {
        Debug.Log("Path: " + FilePath);


        WebClient client = new System.Net.WebClient();
        Uri uri = new Uri(FTPHost + "/" + new FileInfo(FilePath).Name);

        client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
        client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
        client.Credentials = new System.Net.NetworkCredential(FTPUserName, FTPPassword);
        client.UploadFileAsync(uri, "STOR", FilePath);
    }

    private void OnFileUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
    {
        Debug.Log("Uploading Progreess: " + e.ProgressPercentage);
    }

    private void OnFileUploadCompleted(object sender, UploadFileCompletedEventArgs e)
    {
        Debug.Log("File Uploaded");
    }
}