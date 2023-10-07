using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class DB_Request
{
    public string errorMsg = "";
    public Dictionary<string, string> response = new Dictionary<string, string>();

    public bool getError = false;

    private string requestReturn = "";

    private XmlDocument doc;
    private XmlNodeList responseNodeList;
    private XmlNode node;

    public IEnumerator Request(string url, WWWForm form)
    {
        this.errorMsg = "";
        this.response.Clear();
        this.getError = false;

        UnityWebRequest www = UnityWebRequest.Post("https://furyndbgames.000webhostapp.com/sqlconnect/" + url, form);
        yield return www.SendWebRequest();

        this.requestReturn = www.downloadHandler.text;
        www.Dispose();

        doc = new XmlDocument();
        doc.LoadXml(this.requestReturn);
        responseNodeList = doc.SelectNodes("response")[0].ChildNodes;

        for (int i = 0; i < responseNodeList.Count; i++)
        {
            XmlNode node = responseNodeList[i];
            this.response.Add(node.Name, node.InnerText);
        }

        if ( this.response["error"] == "1")
        {
            this.getError = true;
            this.errorMsg = this.response["message"];
        }
    }
}
