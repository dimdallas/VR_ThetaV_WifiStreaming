﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace ThetaStreaming.Scripts
{
	public class ThetaVApMode : MonoBehaviour {

		private bool isLooping = true;
		private Renderer myRenderer;
		public string thetaUrl = "http://192.168.1.1:80";
		private string executeCmd = "/osc/commands/execute";
		Texture2D tex; //= new Texture2D(2, 2);
		Texture2D tex2; //= new Texture2D(2, 2);
		bool TexStored = false;

		// Use this for initialization
		IEnumerator Start () {
			myRenderer = GetComponent<Renderer>();

			string url = thetaUrl + executeCmd;
			var request = HttpWebRequest.Create(url);
			request.Method = "POST";
			request.Timeout = (int)(30 * 10000f);
			request.ContentType = "application/json;charset=utf-8";

			byte[] postBytes = Encoding.Default.GetBytes("{" +
			                                             "\"name\" : \"camera.getLivePreview\"" +
			                                             "}");

			request.ContentLength = postBytes.Length;

			Stream reqStream = request.GetRequestStream ();
			reqStream.Write (postBytes, 0, postBytes.Length);
			reqStream.Close ();
			Stream stream = request.GetResponse ().GetResponseStream ();

			BinaryReader reader = new BinaryReader (new BufferedStream (stream), new System.Text.ASCIIEncoding ());

			List<byte> imageBytes = new List<byte> ();
			bool isLoadStart = false;
			while( isLooping ) { 
				byte byteData1 = reader.ReadByte ();
				byte byteData2 = reader.ReadByte ();

				if (!isLoadStart) {
					if (byteData1 == 0xFF && byteData2 == 0xD8){
						// mjpeg start! ( [0xFF 0xD8 ... )
						imageBytes.Add(byteData1);
						imageBytes.Add(byteData2);

						isLoadStart = true;
					}
				} else {
					imageBytes.Add(byteData1);
					imageBytes.Add(byteData2);

					if (byteData1 == 0xFF && byteData2 == 0xD9){
						// mjpeg end (... 0xFF 0xD9] )

						if (TexStored)
						{
							tex2 = new Texture2D(2, 2);
							tex2.LoadImage((byte[])imageBytes.ToArray());
							myRenderer.material.mainTexture = tex2;
							Destroy(tex);
							TexStored = false;
						}
						else
						{
							tex = new Texture2D(2, 2);
							tex.LoadImage((byte[])imageBytes.ToArray());
							myRenderer.material.mainTexture = tex;
							Destroy(tex2);
							TexStored = true;
						}
						imageBytes.Clear();
						yield return null;
						isLoadStart = false;
					}
				}
			}
		}

		// Update is called once per frame
		void Update () {
			if ( Input.GetKey(KeyCode.Escape) ) {
				isLooping = false;
			}
		}
	}
}
