using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameDevKit
{
    public class GoogleSheetsDownloader
    {
        public enum FileType { tsv, csv }
        private const string URL_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/edit#gid={1}";
        private const string URL_EXPORT_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/export?format={1}&gid={2}";
        private const float DefaultWaitTime = 1f;

        private readonly string _tableId;
        private readonly FileType _fileType;
        private readonly string _sheetId; //gid

        public string DownloadUrl => URL_EXPORT_FORMAT.Format(_tableId, _fileType, _sheetId);
        public string URL => string.Format(URL_FORMAT, _tableId, _sheetId);

        public string DownloadedDataString { get; private set; }
        public string ErrorMessage { get; private set; }
        public float Progress { get; private set; }

        /// <summary> Get from URL: docs.google.com/spreadsheets/d/{TABLE_ID}/edit#gid={SHEET_ID} </summary>
        public GoogleSheetsDownloader(string tableId, string sheetId = "0", FileType fileType = FileType.tsv)
        {
            _tableId = tableId;
            _sheetId = sheetId;
            _fileType = fileType;
        }

        public static string GetUrl(string tableId, string sheetId)
        {
            string url = string.Format(URL_FORMAT, tableId, sheetId);
            return url;
        }

        public static string GetDownloadUrl(string tableId, string sheetId)
        {
            string url = string.Format(URL_EXPORT_FORMAT, tableId, sheetId);
            return url;
        }

        public async UniTask<string> FetchData()
        {
            Debug.LogWarning("Downloading from: " + DownloadUrl);
            using var webRequest = UnityWebRequest.Get(DownloadUrl);
            var requestProgress = webRequest.SendWebRequest();
            Progress = 0;
            while (!requestProgress.isDone)
            {
                if (Progress < 0.9f) { Progress += Time.deltaTime / DefaultWaitTime; }
                await UniTask.Yield();
            }

            Progress = 1;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                ErrorMessage = webRequest.error;
                Debug.LogError("Download Error: " + webRequest.error);
                return webRequest.error;
            }

            var dataString = webRequest.downloadHandler.text;

            DownloadedDataString = dataString;

            Debug.LogWarning("Download finished!");
            return dataString;
        }

        public static int ColumnOf(string name, bool startsAt0 = true)
        {
            const int ALPHABET_COUNT = 26;
            const int CHAR_INDEX_TO_ALPHABET_INDEX = 64;
            int column = 0;
            name = name.ToUpper();
            char[] chars = name.ToCharArray();

            var firstCharIndex = GetCharIndex(chars[0]);
            switch (chars.Length)
            {
                case 1:
                    column = firstCharIndex;
                    break;
                case 2:
                    var secondCharIndex = GetCharIndex(chars[1]);
                    column = (firstCharIndex + 1 * ALPHABET_COUNT) + secondCharIndex;
                    break;
            }

            return column;

            int GetCharIndex(char c)
            {
                return c - CHAR_INDEX_TO_ALPHABET_INDEX - (startsAt0 ? 1 : 0);
            }
        }
    }
}