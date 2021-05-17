using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ThetaStreaming.Scripts
{
    public class ThetaVClientMode : MonoBehaviour {
        [SerializeField]
        private bool isLooping = true;
        private Renderer myRenderer;
        public Material thetaMaterial;
        public string thetaUrl = "http://192.168.1.219";
        private string executeCmd = "/osc/commands/execute";
        public string thetaID = "THETAYL00245200";
        public string thetaPassword = "00245200";
        
        public int texWidth = 1024;
        public int texHeight = 512;
        [SerializeField]
        private Texture2D tex;
        [SerializeField]
        private Texture2D tex2;
        private bool texStored = false;

        private void Start()
        {
            StartCoroutine(GetLivePreview());
        }

        private HttpWebRequest WebRequest(byte[] postbytes)
        {
            return null;
        }

        // Use this for initialization
        IEnumerator GetLivePreview () {
            string url = thetaUrl + executeCmd;

            //Digest Auth
            var credentialCache = new CredentialCache();
            credentialCache.Add(
                new System.Uri(url),
                "Digest",
                new NetworkCredential(thetaID, thetaPassword)
            );

            // myRenderer = GetComponent<Renderer>();

            byte[] postBytes = Encoding.Default.GetBytes("{\"name\" : \"camera.getLivePreview\"}");
            Debug.Log(System.Text.Encoding.Default.GetString(postBytes));
            
            byte[] paramBytes = Encoding.Default.GetBytes("{"+
                                                          "\"name\":\"camera.setOptions\","+
                                                          "\"parameters\":{"+
                                                          "\"options\":{"+
                                                          "\"previewFormat\":{"+
                                                          "\"framerate\": 30,"+
                                                          "\"height\":512,"+
                                                          "\"width\":1024"+"}}}}");
            Debug.Log(System.Text.Encoding.Default.GetString(paramBytes));
            
            var request = HttpWebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Credentials = credentialCache;
            request.Timeout = (int)(30 * 10000f);
            request.Method = "POST";
            System.Net.ServicePointManager.Expect100Continue = false;
            request.ContentType = "application/json;charset=utf-8";

            request.ContentLength = postBytes.Length;
            Stream reqStream = request.GetRequestStream();

            reqStream.Write(postBytes, 0, postBytes.Length);
            float time = 0f;
            using (var stream = request.GetResponse().GetResponseStream())
            {
                BinaryReader reader = new BinaryReader(new BufferedStream(stream), new System.Text.ASCIIEncoding());
                {
                    List<byte> imageBytes = new List<byte>();
                    bool isLoadStart = false;
                    while (isLooping)
                    {
                        byte byteData1 = reader.ReadByte();
                        byte byteData2 = reader.ReadByte();

                        if (!isLoadStart)
                        {
                            // mjpeg start! ( [0xFF 0xD8 ... )
                            if (byteData1 == 0xFF && byteData2 == 0xD8)
                            {
                                imageBytes.Add(byteData1);
                                imageBytes.Add(byteData2);

                                isLoadStart = true;
                            }
                        }
                        else
                        {
                            imageBytes.Add(byteData1);
                            imageBytes.Add(byteData2);

                            // mjpeg end (... 0xFF 0xD9] )
                            if (byteData1 == 0xFF && byteData2 == 0xD9)
                            {
                                // if (TexStored)
                                // {
                                //     tex2 = new Texture2D(2, 2);
                                //     tex2.LoadImage((byte[])imageBytes.ToArray());
                                //     // myRenderer.material.mainTexture = tex2;
                                //     thetaView.mainTexture = tex2;
                                //     Destroy(tex);
                                //     TexStored = false;
                                // }
                                // else
                                // {
                                //     tex = new Texture2D(2, 2);
                                //     tex.LoadImage((byte[])imageBytes.ToArray());
                                //     // myRenderer.material.mainTexture = tex2;
                                //     thetaView.mainTexture = tex;
                                //     Destroy(tex2);
                                //     TexStored = true;
                                // }
                                tex = new Texture2D(texWidth,texHeight);
                                tex.LoadImage((byte[])imageBytes.ToArray());
                                thetaMaterial.mainTexture = tex;
                                imageBytes.Clear();
                                yield return null;
                                isLoadStart = false;
                                Debug.Log(Time.realtimeSinceStartup - time);
                                time = Time.realtimeSinceStartup;
                            }
                        }
                        // yield return new WaitForSeconds(0.0000001f);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                isLooping = !isLooping;
            }
        }
    }
}
